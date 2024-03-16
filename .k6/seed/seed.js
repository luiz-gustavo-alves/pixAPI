const dotenv = require("dotenv");
const fs = require("fs");
const { faker } = require("@faker-js/faker");
const { exit } = require("process");

dotenv.config();

const knex = require("knex")({
  client: "pg",
  connection: process.env.DATABASE_URL,
});

const USERS = 500_000;
const PAYMENT_PROVIDERS = 500_000;
const PAYMENT_PROVIDERS_ACCOUNTS = 500_000;
const PIX_KEYS = 500_000;

const DELETE_DATA = true;
const GENERATE_VALID_ENTRIES = true;
const GENERATE_USERS = true;
const GENERATE_PAYMENT_PROVIDERS = true;
const GENERATE_PAYMENT_PROVIDER_ACCOUNTS = true;
const GENERATE_PIX_KEYS = true;

function getDataFromPixKeyTypeEnum(value) {
  const typeToEnum = {
    "CPF": 0,
    "Email": 1,
    "Phone": 2,
    "Random": 3,
  };
  return typeToEnum[value];
}

async function generateValidEntries() {
  console.log(`Generating valid entries...`);
  const VALID_CPF = "51200000000";
  const VALID_BANK_NAME = "validBankName";
  const user = await knex.select('Id').from("User").where('CPF', VALID_CPF);
  if (user.length === 0) {
    console.log("Generating VALID USER...");
    const VALID_USER = [{ CPF: VALID_CPF, Name: faker.internet.userName() }];
    await populateDataInDatabase(VALID_USER, "User");
    generateJson("./seed/valid_user.json", VALID_USER);
  }

  let bank = await knex.select('Id').from("PaymentProvider").where('BankName', VALID_BANK_NAME);
  if (bank.length === 0) {
    console.log("Generating VALID BANK...");
    const VALID_BANK = [{ Token: faker.string.uuid(64), BankName: VALID_BANK_NAME }];
    await populateDataInDatabase(VALID_BANK, "PaymentProvider");
    generateJson("./seed/valid_bank.json", VALID_BANK);
  }

  const userId = await knex.select('Id').from("User").where('CPF', VALID_CPF);
  bank = await knex.select('Id', 'Token').from("PaymentProvider").where('BankName', VALID_BANK_NAME);
  
  account = await knex.select('Id').from("PaymentProviderAccount").where('UserId', userId[0].Id);
  if (account.length === 0) {
    const VALID_ACCOUNT = [
      {
        UserId: Number(userId[0].Id),
        BankId: Number(bank[0].Id),
        Number: faker.string.numeric(8),
        Agency: faker.string.numeric(4),
      }
    ];
    await populateDataInDatabase(VALID_ACCOUNT, "PaymentProviderAccount");
    generateJson("./seed/valid_account.json", VALID_ACCOUNT);
  }

  account = await knex.select('Id').from("PaymentProviderAccount").where('UserId', userId[0].Id);
  const pixKey = await knex.select('Id').from("PixKey").where('Value', VALID_CPF);
  if (pixKey.length === 0) {
    const VALID_PIX_KEY = [
      {
        PaymentProviderAccountId: Number(account[0].Id),
        Type: getDataFromPixKeyTypeEnum("CPF"),
        Value: VALID_CPF,
      }
    ];
    const VALID_PIX_KEY_JSON = [
      {
        PaymentProviderAccountId: Number(account[0].Id),
        Type: "CPF",
        Value: VALID_CPF,
      }
    ];
    await populateDataInDatabase(VALID_PIX_KEY, "PixKey");
    generateJson("./seed/valid_pixKey.json", VALID_PIX_KEY_JSON);
  }
}

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
    const value = Date.now().toString() + i.toString();
    paymentProviders.push({
      Token: value,
      BankName: value,
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

async function generatePixKeys() {
  console.log(`Generating ${PIX_KEYS} pix keys...`);
  const VALID_ACCOUNT = JSON.parse(fs.readFileSync("./seed/valid_account.json"));
  const accountId = await knex.select('Id').from("PaymentProviderAccount").where('UserId', VALID_ACCOUNT[0].UserId);
  const pixTypes = ["CPF", "Email", "Phone", "Random"];
  const pixKeys = [];
  const pixKeyJSON = [];
  for (let i = 0; i < PIX_KEYS; i++) {
    const value = i.toString();
    pixKeys.push({
      PaymentProviderAccountId: accountId[0].Id,
      Type: getDataFromPixKeyTypeEnum(pixTypes[i % 4]),
      Value: value,
    });
    pixKeyJSON.push({
      Type: pixTypes[i % 4],
      Value: value,
    });
  }
  return { pixKeys, pixKeyJSON };
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
    await knex("Payments").del();
    await knex("PixKey").del();
    await knex("PaymentProviderAccount").del();
    await knex("PaymentProvider").del();
    await knex("User").del();
  }

  const start = new Date();

  if (GENERATE_VALID_ENTRIES) {
    await generateValidEntries();
  }

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
    generateJson("./seed/existing_paymentProviderAccounts.json", paymentProviders);
  }

  if (GENERATE_PIX_KEYS) {
    const { pixKeys, pixKeyJSON } = await generatePixKeys();
    await populateDataInDatabase(pixKeys, "PixKey");
    generateJson("./seed/existing_pixKeys.json", pixKeyJSON);
  }

  console.log("Closing DB connection...");
  await knex.destroy();

  const end = new Date();
  console.log("Done!");
  console.log(`Finished in ${(end - start) / 1000} seconds`);
  exit(0);
}

run();
