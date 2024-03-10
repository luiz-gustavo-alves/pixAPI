# Pix API

O projeto envolve a criação/processamento de Pix para as intituições financeiras credenciada pelo Banco Central (chamado de Provedor de Serviço de Pagamento - PSP).

Lista de ferramentas e tecnologias utilizadas no desenvolvimento do projeto:
- C# e .NET;
- Entity Framework (EF) como ORM;
- Postgres como banco de dados relacional;
- Teste de carga utilizando Grafana k6;
- Monitoramento da aplicação/banco de dados com Prometheus e Grafana;
- Docker.

## Funcionalidades
- **Criação de chave Pix**: Cria uma nova chave Pix para um usuário associado a uma conta numa PSP.
- **Listagem de chave Pix**: Recupera as informações de uma determinada chave Pix.

## Instruções para Executar o Projeto
- Clone este repositório com o comando `git clone https://github.com/luiz-gustavo-alves/pixAPI.git`;
- Utilize o comando `dotnet watch` para subir a aplicação;
- Em caso de sucesso, a aplicação estará rodando através da URL: [http://localhost:5180](http://localhost:5180).

## Instruções para Executar os Testes de Carga
- Abre dois terminais shell;
- No primeiro terminal shell, utilize o comando `dotnet watch` para subir a aplicação;
- No segundo terminal shell, acesse a pasta **.k6** utilizando o comando `cd .k6`;
- Utilize o comando `npm run pretest` para gerar a _seed_ no banco de dados e _payload_ das requisições para o testes de carga;
- Lista de testes implementados:
  - **npm run test:health**: Teste de carga para o Endpoint **GET /health**;
  - **npm run test:getPixKey**: Teste de carga para o Endpoint **GET /keys/:type/:value**;
  - **npm run test:createPixKey** Teste de carga para o Endpoint **POST /keys**.

## Intruções para Subir os Containers de Monitoramento
- Acesse a pasta **Metrics** utilizando o comando `cd Metrics`;
- Com Docker iniciado, utilize o comando `docker compose up -d` para iniciar os containers das ferramentas Prometheus e Grafana;

## Documentação das rotas da API

### Swagger
A documentação das rotas da API foi feita utilizando Swagger e pode ser vista através deste [link](http://localhost:5180/swagger)

Para acessar o link é necessário subir a aplicação utilizando o comando `dotnet watch`.

<hr />

### Descrição das Rotas

### ![](https://place-hold.it/80x20/26baec/ffffff?text=GET&fontsize=16) /health
Retorna "I'm alive" para verificar se a aplicação está em funcionamento.

### ![](https://place-hold.it/80x20/26ec48/ffffff?text=POST&fontsize=16) /keys
Cria uma nova chave Pix para um usuário associado a uma conta numa PSP.
- A chave Pix pode ser do tipo: `CPF`, `Email`, `Phone` ou `Random`.
    - Um único usuário não deve ter duas chaves do mesmo tipo quando for CPF;
    - A chave do tipo `CPF` tem que ter o mesmo valor do `CPF` do usuário.
    - A chave deve ser necessariamente única, independente da PSP.

**Rota autentificada - Header esperado:**
```JSON
{
  "Authorization": "Bearer tokenPSP"
}
```
**Formato esperado do payload (body) da requisição:**
```JSON
 {
  "key": {
    "value": "Valor da chave",
    "type": "CPF, Email, Phone ou Random"
  },
  "user": {
    "cpf": "Número do CPF"
  },
  "account": {
    "number": "Número da conta bancária",
    "agency": "Número da agência bancária",
  }
}
```

### ![](https://place-hold.it/80x20/26baec/ffffff?text=GET&fontsize=16) /keys/:type/:value
Recupera as informações de uma determinada chave Pix através do tipo (type) e valor (value) da chave.

**Rota autentificada - Header esperado:**
```JSON
{
  "Authorization": "Bearer tokenPSP"
}
```
**Formato esperado da resposta da requisição:**
```JSON
 {
  "key": {
    "value": "Valor da chave",
    "type": "CPF, Email, Phone ou Random"
  },
  "user": {
    "cpf": "Número do CPF"
  },
  "account": {
    "number": "Número da conta bancária",
    "agency": "Número da agência bancária",
    "bankId": "ID da PSP",
    "bankName": "Nome da PSP"
  }
}
```

