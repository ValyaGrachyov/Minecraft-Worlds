# Тестирование генерации мира

Этот документ описывает эндпоинты для тестирования генерации мира со всеми этапами: Terrain, Caves, Fluids и Surface.

## Эндпоинты

### 1. Полная генерация чанка со статистикой

**GET** `/api/WorldGenerationTest/chunk`

Генерирует чанк со всеми этапами и возвращает статистику по каждому этапу.

**Параметры запроса:**
- `chunkX` (int, по умолчанию: 0) - X координата чанка
- `chunkZ` (int, по умолчанию: 0) - Z координата чанка
- `seed` (long, по умолчанию: 12345) - Сид для генерации

**Пример запроса:**
```
GET /api/WorldGenerationTest/chunk?chunkX=0&chunkZ=0&seed=12345
```

**Пример ответа:**
```json
{
  "chunkPosition": {
    "chunkX": 0,
    "chunkZ": 0
  },
  "seed": 12345,
  "stages": [
    {
      "stage": "Terrain",
      "blockCounts": {
        "Air": 12345,
        "Stone": 67890
      },
      "totalBlocks": 98304
    },
    {
      "stage": "Caves",
      "blockCounts": {
        "Air": 23456,
        "Stone": 56789
      },
      "totalBlocks": 98304
    },
    {
      "stage": "Fluids",
      "blockCounts": {
        "Air": 20000,
        "Stone": 56789,
        "Water": 3456
      },
      "totalBlocks": 98304
    },
    {
      "stage": "Surface",
      "blockCounts": {
        "Air": 20000,
        "Stone": 50000,
        "Dirt": 5000,
        "Grass": 2000,
        "Water": 3456
      },
      "totalBlocks": 98304
    }
  ],
  "finalStats": { ... },
  "sliceVisualization": {
    "y": 64,
    "visualization": "...",
    "legend": { ... }
  }
}
```

### 2. Вертикальный срез чанка

**GET** `/api/WorldGenerationTest/vertical-slice`

Генерирует чанк и возвращает вертикальный срез (X=8, все Y и Z).

**Параметры запроса:**
- `chunkX` (int, по умолчанию: 0)
- `chunkZ` (int, по умолчанию: 0)
- `sliceX` (int, по умолчанию: 8) - X координата среза
- `seed` (long, по умолчанию: 12345)

**Пример запроса:**
```
GET /api/WorldGenerationTest/vertical-slice?chunkX=0&chunkZ=0&sliceX=8&seed=12345
```

**Пример ответа:**
```json
{
  "chunkPosition": { "chunkX": 0, "chunkZ": 0 },
  "sliceX": 8,
  "seed": 12345,
  "visualization": "320 | ~~~~~~~~~~~~~~\n319 | ~~~~~~~~~~~~~~\n...",
  "legend": {
    " ": "Air",
    "#": "Stone",
    "=": "Dirt",
    "+": "Grass",
    "~": "Water"
  }
}
```

### 3. Горизонтальный срез чанка

**GET** `/api/WorldGenerationTest/horizontal-slice`

Генерирует чанк и возвращает горизонтальный срез на указанной высоте.

**Параметры запроса:**
- `chunkX` (int, по умолчанию: 0)
- `chunkZ` (int, по умолчанию: 0)
- `sliceY` (int, по умолчанию: 64) - Y координата среза
- `seed` (long, по умолчанию: 12345)

**Пример запроса:**
```
GET /api/WorldGenerationTest/horizontal-slice?chunkX=0&chunkZ=0&sliceY=64&seed=12345
```

### 4. Статистика по этапам

**GET** `/api/WorldGenerationTest/stats`

Возвращает статистику по блокам на каждом этапе генерации.

**Параметры запроса:**
- `chunkX` (int, по умолчанию: 0)
- `chunkZ` (int, по умолчанию: 0)
- `seed` (long, по умолчанию: 12345)

**Пример запроса:**
```
GET /api/WorldGenerationTest/stats?chunkX=0&chunkZ=0&seed=12345
```

## Легенда символов

- ` ` (пробел) - Air (воздух)
- `#` - Stone (камень)
- `=` - Dirt (земля)
- `+` - Grass (трава)
- `~` - Water (вода)

## Этапы генерации

1. **Terrain** - Генерация базового ландшафта (камень/воздух на основе плотности)
2. **Caves** - Вырезание пещер (сырные, спагетти, лапшичные)
3. **Fluids** - Заполнение жидкостями (вода в океанах и подземных полостях)
4. **Surface** - Построение поверхности (замена камня на траву, землю и т.д.)

## Примеры использования

### Проверка работы всех этапов

```bash
# Полная генерация с визуализацией
curl "http://localhost:5000/api/WorldGenerationTest/chunk?chunkX=0&chunkZ=0&seed=12345"

# Вертикальный срез для проверки пещер
curl "http://localhost:5000/api/WorldGenerationTest/vertical-slice?sliceX=8&seed=12345"

# Горизонтальный срез на уровне моря
curl "http://localhost:5000/api/WorldGenerationTest/horizontal-slice?sliceY=63&seed=12345"

# Статистика по этапам
curl "http://localhost:5000/api/WorldGenerationTest/stats?seed=12345"
```

### Проверка разных чанков

```bash
# Чанк (0, 0)
curl "http://localhost:5000/api/WorldGenerationTest/chunk?chunkX=0&chunkZ=0"

# Чанк (1, 0)
curl "http://localhost:5000/api/WorldGenerationTest/chunk?chunkX=1&chunkZ=0"

# Чанк (0, 1)
curl "http://localhost:5000/api/WorldGenerationTest/chunk?chunkX=0&chunkZ=1"
```

### Проверка с разными сидами

```bash
# Сид 12345
curl "http://localhost:5000/api/WorldGenerationTest/chunk?seed=12345"

# Сид 54321
curl "http://localhost:5000/api/WorldGenerationTest/chunk?seed=54321"
```

## Что проверять

1. **Terrain**: Должен быть камень и воздух, рельеф должен быть плавным
2. **Caves**: После генерации пещер должно быть больше воздуха
3. **Fluids**: В океанах должна появиться вода, подземные полости ниже уровня моря должны заполниться водой
4. **Surface**: На поверхности должен появиться Grass, под ним Dirt, в океанах должна остаться вода

## Swagger UI

Все эндпоинты доступны в Swagger UI по адресу:
```
http://localhost:5000/swagger
```

