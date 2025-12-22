using System.Buffers.Binary;
using System.IO.Compression;
using fNbt;
using MinecraftWorldsAPI.Models;

namespace MinecraftWorldsAPI.Services;

public record WorldExportOption(string WorldName, long Seed = 1, int StartX = 0, int StartY = 100, int StartZ = 0);

public static class Converter
{
    private const int RegionSizeX = 32;
    private const int RegionSizeZ = 32;

    private const int BlocksPerSection = Chunk.SizeX * Chunk.SectionSizeY * Chunk.SizeZ;
    private const int NibbleArrayBytesPerSection = BlocksPerSection / 2;

    private const int BytesPerLocationEntry = 4;
    private const int BytesPerTimestampEntry = 4;

    private const int LocationTableBytes = RegionSizeX * RegionSizeZ * BytesPerLocationEntry;
    private const int TimestampTableBytes = RegionSizeX * RegionSizeZ * BytesPerTimestampEntry;

    private const int SectorBytes = LocationTableBytes;
    private const int RegionHeaderBytes = LocationTableBytes + TimestampTableBytes;
    private const int RegionHeaderSectors = RegionHeaderBytes / SectorBytes;
    
    public static async Task ConvertAsync(
        Stream outputStream,
        IEnumerable<Chunk> chunks,
        WorldExportOption options,
        CancellationToken ct
    )
    {
        chunks = (chunks as List<Chunk>) ?? chunks.ToList();
        
        await using var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);

        WriteZipEntry(zipArchive, "level.dat", BuildLevelDat(options));
        WriteZipEntry(zipArchive, "session.lock", BuildSessionLock());

        var regions = chunks
            .GroupBy(ch => (RegionX: FloorDiv(ch.Position.X, RegionSizeX), RegionZ: FloorDiv(ch.Position.Z, RegionSizeZ)));

        foreach (var region in regions)
        {
            ct.ThrowIfCancellationRequested();

            var regionBytes = await BuildRegionDat(region);
            var regionPath = $"region/r.{region.Key.RegionX}.{region.Key.RegionZ}.mca";
            WriteZipEntry(zipArchive, regionPath, regionBytes);
        }

        await outputStream.FlushAsync(ct);
    }

    private static void WriteZipEntry(ZipArchive zipArchive, string path, byte[] bytes)
    {
        var entry = zipArchive.CreateEntry(path);
        using var entryStream = entry.Open();
        entryStream.Write(bytes, 0, bytes.Length);
    }

    private static byte[] BuildLevelDat(WorldExportOption options)
    {
        var data = new NbtCompound("Data")
        {
            new NbtString("LevelName", options.WorldName),
            new NbtLong("RandomSeed", options.Seed),

            new NbtInt("SpawnX", options.StartX),
            new NbtInt("SpawnY", options.StartY),
            new NbtInt("SpawnZ", options.StartZ),

            new NbtInt("GameType", 1),
            new NbtByte("hardcore", 0),
            new NbtByte("Difficulty", 1),
            new NbtByte("DifficultyLocked", 0),
            new NbtByte("allowCommands", 1),
            new NbtByte("initialized", 1),

            new NbtLong("Time", 0),
            new NbtLong("DayTime", 0),

            new NbtString("generatorName", "flat"),
            new NbtInt("generatorVersion", 0),
            new NbtString("generatorOptions", "3;minecraft:air;1;"),

            new NbtLong("LastPlayed", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            new NbtByte("MapFeatures", 0),
            new NbtLong("SizeOnDisk", 0),

            new NbtInt("version", 19133),

            new NbtCompound("Version")
            {
                new NbtInt("Id", 1343),
                new NbtString("Name", "1.12.2"),
                new NbtByte("Snapshot", 0)
            }
        };

        var root = new NbtCompound("") { data };

        var file = new NbtFile(root);
        using var memoryStream = new MemoryStream();
        using (var gz = new GZipStream(memoryStream, CompressionLevel.SmallestSize, true))
            file.SaveToStream(gz, NbtCompression.None);
        return memoryStream.ToArray();
    }

    private static byte[] BuildSessionLock()
    {
        var buf = new byte[8];
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        BinaryPrimitives.WriteInt64BigEndian(buf, now);
        return buf;
    }

    private static async Task<byte[]> BuildRegionDat(IEnumerable<Chunk> chunks)
    {
        var header = new byte[RegionHeaderBytes];

        var sectorData = new List<byte>(SectorBytes * 4);
        var nextFreeFactor = RegionHeaderSectors;

        var unixTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var chunk in chunks)
        {
            int localX = Mod(chunk.Position.X, RegionSizeX),
                localZ = Mod(chunk.Position.Z, RegionSizeZ),
                headerIndex = localX + localZ * RegionSizeX;

            var chunkNbt = BuildChunkNbt(chunk);
            var chunkBytes = SerializeNbtUncompressed(chunkNbt);

            byte[] compressed;
            using (var ms = new MemoryStream())
            {
                await using (var z = new ZLibStream(ms, CompressionLevel.Fastest, true))
                    await z.WriteAsync(chunkBytes);
                compressed = ms.ToArray();
            }

            var length = compressed.Length + 1;
            var record = new byte[4 + 1 + compressed.Length];
            BinaryPrimitives.WriteInt32BigEndian(record, length);
            record[4] = 2; // ZLib
            Buffer.BlockCopy(compressed, 0, record, 5, compressed.Length);

            var sectorsNeeded = (record.Length + SectorBytes - 1) / SectorBytes;
            var offsetSector = nextFreeFactor;
            
            sectorData.AddRange(record);
            var pad = sectorsNeeded * SectorBytes - record.Length;
            if (pad > 0) sectorData.AddRange(new byte[pad]);

            nextFreeFactor += sectorsNeeded;

            var locPos = headerIndex * BytesPerLocationEntry;
            header[locPos + 0] = (byte)((offsetSector >> 16) & 0xFF);
            header[locPos + 1] = (byte)((offsetSector >> 8) & 0xFF);
            header[locPos + 2] = (byte)(offsetSector & 0xFF);
            header[locPos + 3] = (byte)sectorsNeeded;

            var tsPos = LocationTableBytes + headerIndex * BytesPerTimestampEntry;
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(tsPos, BytesPerTimestampEntry), unixTime);
        }

        var result = new byte[header.Length + sectorData.Count];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(sectorData.ToArray(), 0, result, header.Length, sectorData.Count);
        return result;
    }

    private static NbtFile BuildChunkNbt(Chunk chunk)
    {
        var level = new NbtCompound("Level")
        {
            new NbtInt("xPos", chunk.Position.X),
            new NbtInt("zPos", chunk.Position.Z),

            new NbtLong("LastUpdate", 0),
            new NbtByte("TerrainPopulated", 1),
            new NbtByte("LightPopulated", 1),
            new NbtLong("InhabitedTime", 0),

            new NbtList("Entities", NbtTagType.Compound),
            new NbtList("TileEntities", NbtTagType.Compound),

            new NbtByteArray("Biomes", BuildBiomes(chunk)),
            new NbtIntArray("HeightMap", BuildHeightMap(chunk)),
        };

        var sections = new NbtList("Sections",  NbtTagType.Compound);
        for (var sectionY = 0; sectionY < Chunk.SectionSizeY; sectionY++)
        {
            var sec = BuildSection(chunk, sectionY);
            if (sec != null) sections.Add(sec);
        }

        level.Add(sections);

        var root = new NbtCompound("") { level };
        return new NbtFile(root);
    }

    private static byte[] BuildBiomes(Chunk chunk)
    {
        return new byte[Chunk.SizeX * Chunk.SizeZ];
        // var res = new byte[Chunk.SizeX * Chunk.SizeZ];
        // for (var x = 0; x < Chunk.SizeX; x++)
        // for (var z = 0; z < Chunk.SizeZ; z++)
        //     res[x + z * Chunk.SizeX] = (byte)chunk.GetBiome(x, z);
        // return res;
    }

    private static int[] BuildHeightMap(Chunk chunk)
    {
        return new int[Chunk.SizeX * Chunk.SizeZ];
        // var res = new int[Chunk.SizeX * Chunk.SizeZ];
        // for (var x = 0; x < Chunk.SizeX; x++)
        // for (var z = 0; z < Chunk.SizeZ; z++)
        //     res[x + z * Chunk.SizeX] = chunk.GetHeight(x, z);
        // return res;
    }

    private static NbtCompound? BuildSection(Chunk chunk, int sectionY)
    {
        var blocksLow = new byte[BlocksPerSection];
        var addHigh = new byte[NibbleArrayBytesPerSection];
        var dataNibbles = new byte[NibbleArrayBytesPerSection];

        var blockLight = new byte[NibbleArrayBytesPerSection];
        var skyLight = new byte[NibbleArrayBytesPerSection];
        Array.Fill(skyLight, (byte)0xFF);

        bool anyNonAir = false,
            anyHighId = false;

        var baseY = sectionY * Chunk.SectionSizeY;

        for (var y = 0; y < Chunk.SectionSizeY; y++)
        {
            var worldY = baseY + y;
            if (worldY < chunk.MinY || worldY > chunk.MaxY) continue;

            for (var z = 0; z < Chunk.SizeZ; z++)
            for (var x = 0; x < Chunk.SizeX; x++)
            {
                var block = chunk[x, worldY, z];

                var index = IndexInSection(x, y, z);

                blocksLow[index] = (byte)((short)block.Block & 0xFF);
                var high = (byte)(((short)block.Block >> 8) & 0xFF);
                if (high != 0)
                {
                    anyHighId = true;
                    SetNibble(addHigh, index, high);
                }

                SetNibble(dataNibbles, index, 0);

                if (block != Block.Air) anyNonAir = true;
            }
        }

        if (!anyNonAir) return null;

        var section = new NbtCompound
        {
            new NbtByte("Y", (byte)sectionY),
            new NbtByteArray("Blocks", blocksLow),
            new NbtByteArray("Data",  dataNibbles),
            new NbtByteArray("BlockLight", blockLight),
            new NbtByteArray("SkyLight", skyLight),
        };

        if (anyHighId)
            section.Add(new NbtByteArray("Add", addHigh));

        return section;
    }

    private static int IndexInSection(int x, int y, int z)
        => x + (z + y * Chunk.SizeZ) * Chunk.SizeX;

    private static void SetNibble(byte[] arr, int index, byte value)
    {
        var i = index >> 1;
        var high = (index & 1) == 1;

        var cur = arr[i];
        if (high) arr[i] = (byte)((cur & 0x0F) | (value << 4));
        else arr[i] = (byte)((cur & 0xF0) | (value & 0x0F));
    }

    private static byte[] SerializeNbtUncompressed(NbtFile file)
    {
        using var ms = new MemoryStream();
        file.SaveToStream(ms, NbtCompression.None);
        return ms.ToArray();
    }

    private static int Mod(int a, int b) => (a % b + b) % b;

    private static int FloorDiv(int a, int b)
    {
        int res = a / b,
            rem = a % b;
        return (rem != 0 && (rem ^ res) < 0) ? res - 1 : res;
    }
}
