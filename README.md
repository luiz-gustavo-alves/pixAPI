# Pix API

"Pix API" is a REST API that allows Payment Providers (PSPs) to create/retrieve "Pix" keys, process payments and check differences to payments file logs from PSPs and API data records.
<br>
<br>
This project was built in C# using ASP.NET Core (version 8.0.2) and it was developed following scalability and performance concepts.

## Tools and Technologies
- C#;
- ASP.NET Core (version 8.0.2);
- Entity Framework;
- Postgres;
- k6 (Load testing);
- Prometheus;
- Grafana;
- Docker;
- RabbitMQ;
- Swagger.

## Features
- **Pix Key Creation**: Creates new "Pix" key for user account related to PSP.
- **Pix key Retrieval**: Retrieves "Pix" key information.
- **Payments**: Process payment based on user origin information and "Pix" key destination.
- **Concilliation**: Check differences to payments file logs from PSPs and API data records.

## How to Install and Run the Project
Clone this repository: `git clone https://github.com/luiz-gustavo-alves/pixAPI.git`
<br>
Access **Docker** folder and run Docker environment:
```bash
cd Docker
docker compose up -d
```

API will be running on [http://localhost:5000](http://localhost:5000/Health)
<br>
Grafana will be running on [http://localhost:3000](http://localhost:3000)
<br>
RabbitMQ Management will be running on [http://localhost:15672](http://localhost:15672)

## How to Run Load Tests
Access **.k6** folder:
```bash
cd ./k6
```

Inside of **.k6** folder, run `npm run pretest` script to generate new _seed_ (data records) and _payload_ (POST requests).
<br>
This script should create JSON files to run load tests:

![image](https://github.com/luiz-gustavo-alves/pixAPI/assets/114351018/f67249cb-0963-404f-ad6f-b69015143c4f)


Inside of **.k6** folder, you can run those following scripts for loading tests:
  - **npm run test:health**: Load test for **GET /health** endpoint;
  - **npm run test:getPixKey**: Load test for **GET /keys/:type/:value** endpoint;
  - **npm run test:createPixKey**: Load test for **POST /keys** endpoint;
  - **npm run test:makePayment**: Load test for **POST /payments** endpoint.

## Swagger
Swagger is an open-source tool that allows to visualize and interact with the API’s resources.
<br>
With active docker containers, Swagger will be running on [http://localhost:5000/swagger](http://localhost:5000/swagger)

<hr />

### Endpoints Description

### ![](https://place-hold.it/80x20/26baec/ffffff?text=GET&fontsize=16) /Health
Returns "I'm alive" - useful to check if API is working.

### ![](https://place-hold.it/80x20/26ec48/ffffff?text=POST&fontsize=16) /keys
Creates new "Pix" key for user account related to PSP.
<br>
Currently available "Pix" types: `CPF`, `Email`, `Phone`, `Random`.

Business Logic:
- User can't have more than two "Pix" keys with same CPF type;
- CPF type should have the same CPF value from user;
- Key should be always unique, regardless of PSPs.

**Auth Endpoint - Expected Header:**
```JSON
{
  "Authorization": "Bearer PSPtoken"
}
```
**Expected Payload Request:**
```JSON
 {
  "key": {
    "value": "Pix key value",
    "type": "CPF, Email, Phone or Random"
  },
  "user": {
    "cpf": "CPF number"
  },
  "account": {
    "number": "User bank account number",
    "agency": "User bank agency number",
  }
}
```

### ![](https://place-hold.it/80x20/26baec/ffffff?text=GET&fontsize=16) /keys/:type/:value
Retrieves "Pix" key information based on key type and value.

**Auth Endpoint - Expected Header:**
```JSON
{
  "Authorization": "Bearer PSPtoken"
}
```
**Expected Request Response:**
```JSON
 {
  "key": {
    "value": "Pix key value",
    "type": "CPF, Email, Phone or Random"
  },
  "user": {
    "name": "User name",
    "maskedCpf": "CPF number - three first and last two digits only"
  },
  "account": {
    "number": "User bank account number",
    "agency": "User bank agency number",
    "bankId": "PSP Id",
    "bankName": "PSP name"
  }
}
```

### ![](https://place-hold.it/80x20/26ec48/ffffff?text=POST&fontsize=16) /payments
Process payment based on user origin information and "Pix" key destination.
<br>
Uses RabbitMQ queues to send payments messages to "pixAPI-Payments-Consumer".

Business Logic:
- User cant make the same payment in a period of time lesser than 30 seconds.

**Auth Endpoint - Expected Header:**
```JSON
{
  "Authorization": "Bearer PSPtoken"
}
```
**Expected Payload Request:**
```JSON
 {
   "origin": {
      "user": {
        "cpf": "CPF number"
      },
      "account": {
         "number": "User bank account number",
         "agency": "User bank agency number",
      },
   },
   "desinty": {
      "key": {
         "value": "Pix key value",
         "type": "CPF, Email, Phone or Random"
      },
   },
   "amount": "Payment amount",
   "description": "Payment description (optional)"
}
```

### ![](https://place-hold.it/80x20/26ec48/ffffff?text=POST&fontsize=16) /concilliation
Check differences to payments file logs from PSPs and API data records.
<br>
Uses RabbitMQ queues to send concilliation messages to "pixAPI-Concilliation-Consumer".

**Auth Endpoint - Expected Header:**
```JSON
{
  "Authorization": "Bearer PSPtoken"
}
```
**Expected Payload Request:**
```JSON
{
   "date": "Date that API data records will use (yyyy-MM-dd format)",
   "file": "PSP payment file log",
   "postback": "Weebhook to notify PSP after concilliation ended"
}
````

## Links

| Description | URL |
| --- | --- |
| PSP Mock | https://github.com/luiz-gustavo-alves/pixAPI-PSP-Mock
| Payment Consumer | https://github.com/luiz-gustavo-alves/pixAPI-Payments-Consumer
| Concilliation Consumer | https://github.com/luiz-gustavo-alves/pixAPI-Concilliation-Consumer
