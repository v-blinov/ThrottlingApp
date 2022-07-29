# ThrottlingApp

## Цель проекта:

Разобраться с идеей rate-limiter при использовании в web api .net приложении через реализацию сврей версии.  

## Описание идеи проекта:

> Ограничение скорости — это процесс ограничения количества запросов к ресурсу в течение определенного временного окна.  
Поставщик услуг, предлагающий API для потребителей, будет иметь ограничения на запросы, сделанные в течение определенного периода времени. Например, каждый уникальный ключ пользователя/IP-адреса/клиента будет иметь ограничение на количество запросов к конечной точке API.  
(см. подробнее) [Codemaze: Rate Limiting in ASP.NET Core Web API](https://code-maze.com/aspnetcore-web-api-rate-limiting/)

Для реализации выбран алгоритм fixed window.  
Клиенты идентифицируются по IP.
Настройки лимитов лучше хранить в отдельном сервисе (БД или Redis, например), в текущей реализации настройки хранятся на уровне приложения, т.е. нет возможности изменить значения в runtime.

Доступные настройки:

- количество одновременных запросов от одного потребителя
- период времени (окно, в течение которого можно сделать только N запросов)
- настройки задаются глобально, при этом возможно:
    - переопределить параметры на методах в контроллере;
    - задать индивидуальные настройки для потребителей.

## Ключевые слова

- dotnet 6
- rate limiter
- user middleware
- user attributes

## Порядок развертывания:

1. Склонировать репозиторий:

    ```bash
    git clone https://github.com/v-blinov/ThrottlingApp.git
    ```

2. Перейти в папку проекта, запустить

    ```bash
    cd src/Basic
    dotnet run
    ```

3. Поиграть с ручками

    - http://localhost:5116/weather/get
    - http://localhost:5116/weather/get2
    - http://localhost:5116/weather/get3
