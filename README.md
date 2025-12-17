# Minecraft World Generator — Генерация мира на основе сидов
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) ![Windows 11](https://img.shields.io/badge/Windows%2011-%230079d5.svg?style=for-the-badge&logo=Windows%2011&logoColor=white) ![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white) ![Swagger](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white) 

##  Описание проекта

Этот проект — система процедурной генерации мира Minecraft, построенная  без использования готовых библиотек генерации мира.  
Генерация полностью детерминирована и управляется **сидом**, переданным пользователем.  
Основная цель — исследовать влияние различных алгоритмов PRNG и шумов на итоговую структуру мира.

Система предоставляет **REST API**, возвращающее .zip-архив, в котором содержатся файлы, описывающие мир майнкрафта.

## Используемые технологии

- **Использование псевдо-случайных чисел**
- **ASP .Net Core**
-  **Docker**
-  **Swagger UI**

## Архитектура проекта

Проект реализован в виде Web Api приложения на ASP .Net Core. Для выделения сервисов используются интерфейсы. Сервис генерации мира реализован с помощью паттерна **Building**


## Запуск проекта

#### Локальный запуск

```bash
git clone https://github.com/ValyaGrachyov/Minecraft-Worlds.git
cd <repo>
dotnet run
```

Далее можно обратиться по url: http://localhost/swagger/index.html , где откроется документация swagger. Далее можно обратиться по эндпоинту:

GET /generate?seed=&algorythm=&cordX=&cordY&chunks=

По умолчанию выбран алгоритм XorShift64Random, для seed значением по умолчанию является текущее время в среде. cordX&cordY отвечают за начальную точку генерации, chunks описывает количество генерируемых чанков от центрального. В ответ придет zip-файл, который можно интегрировать в папку .mnecraft/worlds.

#### Запуск в Docker
```bash
docker build -t minecraft-generator .
docker run -p 5000:80 minecraft-generator
```
Далее можно обратиться по url: http://localhost/swagger/index.html , где откроется документация swagger. Далее можно обратиться по эндпоинту:

GET /generate?seed=&algorythm=&cordX=&cordY&chunks= 

По умолчанию выбран алгоритм XorShift64Random, для seed значением по умолчанию является текущее время в среде. cordX&cordY отвечают за начальную точку генерации, chunks описывает количество генерируемых чанков от центрального. В ответ придет zip-файл, который можно интегрировать в папку .mnecraft/worlds.

### Команда проекта

Гафарова Камилла — гр. 11.1-521

Бариев Раиль — гр. 11.1-521

Грачев Валентин — гр. 11.1-521

Гафеев Глеб — гр. 11.1-522

### Лицензия

GNU GENERAL PUBLIC LICENSE                     Version 3
