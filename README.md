[Eng](#GraphQL-Server-with-HotChocolate) / [Ru](#GraphQL-сервер-с-HotChocolate)
# GraphQL Server with HotChocolate 

This project is a GraphQL server written in C# utilizing the HotChocolate library. It is designed to provide an efficient, flexible API for interacting with a PostgreSQL database, handle complex queries, and facilitate real-time data updates via subscriptions. Additionally, the server integrates with a microservice for sending email notifications via HTTPS requests.

## Features

### GraphQL API:

- Supports Queries, Mutations, and Subscriptions for efficient data management and real-time updates.


### PostgreSQL Integration:

- Seamlessly interacts with a PostgreSQL database for storing and retrieving data.


### Email Notifications:

- Sends email notifications by making HTTPS requests to an external email microservice.


### Extensible and Scalable:

- Designed with scalability in mind, allowing easy integration of additional features and services.



## Technologies Used

- C#: The main programming language.

* HotChocolate: For building the GraphQL server.

* PostgreSQL: As the database for data persistence.

+ HTTPS Requests: To communicate with an email-sending microservice.


## Installation

1. Clone the repository:

&emsp;&emsp;```git clone https://github.com/ShdwKick/your-repository-name.git```


2. Navigate to the project directory:

&emsp;&emsp;```cd RoleGameServer```


3. Restore dependencies:

&emsp;&emsp;```dotnet restore```


4. Configure the database connection string in appsettings.json.


5. Run the server:

&emsp;&emsp;```dotnet run```



## Usage

- GraphQL Playground: Access the GraphQL Playground (usually at /graphql by default) to test queries, mutations, and subscriptions.

* API Endpoints: Use your preferred HTTP client to send GraphQL requests to the server.

+ Email Service Integration: Ensure the email microservice is running and accessible via the configured URL for email notifications to work.


## Contributing

- Contributions are welcome! Please submit a pull request or open an issue for any suggestions or bug reports.

<br/><br/><br/>

# GraphQL сервер с HotChocolate

Этот проект представляет собой GraphQL сервер, написанный на C# с использованием библиотеки HotChocolate. Сервер предоставляет эффективный и гибкий API для взаимодействия с базой данных PostgreSQL, обработки сложных запросов и поддержки подписок для работы в реальном времени. Также сервер интегрируется с микросервисом для отправки почтовых уведомлений через HTTPS-запросы.

## Возможности

### GraphQL API:

- Поддержка запросов, мутаций и подписок для работы с данными и обновлений в реальном времени.


### Интеграция с PostgreSQL:

- Взаимодействие с базой данных PostgreSQL для хранения и получения данных.


### Отправка почтовых уведомлений:

- Отправка email-уведомлений через HTTPS-запросы к внешнему микросервису.


### Расширяемость и масштабируемость:

- Спроектирован для масштабирования и интеграции дополнительных функций и сервисов.



## Используемые технологии

- C#: Основной язык программирования.

* HotChocolate: Для создания GraphQL-сервера.

* PostgreSQL: База данных для хранения данных.

+ HTTPS-запросы: Для взаимодействия с микросервисом отправки email.


## Установка

1. Клонируйте репозиторий:

&emsp;&emsp;```git clone https://github.com/ShdwKick/your-repository-name.git```


2. Перейдите в папку проекта:

&emsp;&emsp;```cd your-repository-name```


3. Восстановите зависимости:

&emsp;&emsp;```dotnet restore```


4. Настройте строку подключения к базе данных в файле appsettings.json.


5. Запустите сервер:

&emsp;&emsp;```dotnet run```



## Использование

- GraphQL Playground: Доступ к GraphQL Playground (обычно по адресу /graphql) для тестирования запросов, мутаций и подписок.

* API Эндпоинты: Используйте HTTP-клиент для отправки GraphQL-запросов к серверу.

+ Интеграция email-сервиса: Убедитесь, что микросервис для отправки email работает и доступен по настроенному URL.


## Участие в разработке

- Приветствуются любые предложения и исправления! Вы можете отправить pull request или открыть issue для обсуждения.

