const dotenv = require("dotenv");
const fs = require("fs");
const { faker } = require("@faker-js/faker");

dotenv.config();

const knex = require("knex")({
  client: "pg",
  connection: process.env.DATABASE_URL,
});

const USERS = 1_000_000;
const PAYMENT_PROVIDERS = 1_000_000;
const PAYMENT_PROVIDERS_ACCOUNTS = 1_000_000;

const DELETE_DATA = true;
const GENERATE_USERS = true;
const GENERATE_PAYMENT_PROVIDERS = true;
const GENERATE_PAYMENT_PROVIDER_ACCOUNTS = true;

function generateUsers() {
  console.log(`Generating ${USERS} users...`);
  const users = [];
  for (let i = 0; i < USERS; i++) {
    const BASE_CPF = 10000000000;
    const strNum = i.toString();
    const randomCPF = BASE_CPF.toString().slice(0, -strNum.length) + strNum;
    users.push({
      CPF: randomCPF,
      Name: faker.internet.userName()
    });
  }
  return users;
}

function generatePaymentProviders() {
  console.log(`Generating ${PAYMENT_PROVIDERS} payment providers...`);
  const paymentProviders = [];
  for (let i = 0; i < PAYMENT_PROVIDERS; i++) {
    const timestamp = Date.now().toString();
    const randomString = `${faker.string.nanoid(64 - timestamp.length).concat(timestamp)}`;
    paymentProviders.push({
      Token: randomString,
      BankName: randomString,
    });
  }
  return paymentProviders;
}

async function generatePaymentProviderAccounts() {
  console.log(`Generating ${PAYMENT_PROVIDERS_ACCOUNTS} payment providers accounts...`);
  const users = await knex.select('Id').from("User");
  const paymentProviders = await knex.select('Id').from("PaymentProvider");
  const paymentProviderAccounts = [];
  for (let i = 0; i < PAYMENT_PROVIDERS_ACCOUNTS; i++) {
    paymentProviderAccounts.push({
      UserId: Number(users[i].Id),
      BankId: Number(paymentProviders[i].Id),
      Number: faker.string.numeric(8),
      Agency: faker.string.numeric(4),
    });
  }
  return paymentProviderAccounts;
}

async function populateDataInDatabase(data, tableName) {
  console.log("Storing on DB...");
  await knex.batchInsert(tableName, data);
}

function generateJson(filepath, data) {
  if (fs.existsSync(filepath)) {
    fs.unlinkSync(filepath);
  }
  fs.writeFileSync(filepath, JSON.stringify(data));
}

async function run() {
  if (DELETE_DATA) {
    console.log("Deleting data...")
    await knex("User").del();
  }

  const start = new Date();

  if (GENERATE_USERS) {
    const users = generateUsers();
    await populateDataInDatabase(users, "User");
    generateJson("./seed/existing_users.json", users);
  }

  if (GENERATE_PAYMENT_PROVIDERS) {
    const paymentProviders = generatePaymentProviders();
    await populateDataInDatabase(paymentProviders, "PaymentProvider");
    generateJson("./seed/existing_paymentProviders.json", paymentProviders);
  }

  if (GENERATE_PAYMENT_PROVIDER_ACCOUNTS) {
    const paymentProviders = await generatePaymentProviderAccounts();
    await populateDataInDatabase(paymentProviders, "PaymentProviderAccount");
    generateJson("./seed/existing_paymentProviderAccount.json", paymentProviders);
  }

  console.log("Closing DB connection...");
  await knex.destroy();

  const end = new Date();
  console.log("Done!");
  console.log(`Finished in ${(end - start) / 1000} seconds`);
}

run();
