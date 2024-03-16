const fs = require("fs");
const { faker } = require("@faker-js/faker");
const { exit } = require("process");

const PIX_KEY_PAYLOAD = 100000;
const PAYMENT_PAYLOAD = 100000;

const GENERATE_CREATE_PIX_KEY = true;
const GENERATE_PAYMENT_PAYLOAD = true;

function generateCreatePixKeyPayload() {
  console.log(`Generating ${PIX_KEY_PAYLOAD} createPixKeys payload...`);
  const createPixKeyPayload = [];
  for (let i = 0; i < PIX_KEY_PAYLOAD; i++) {
    createPixKeyPayload.push({
      key: {
        type: "Random",
        value: faker.string.uuid(64),
      },
      account: {
        number: faker.string.numeric(8),
        agency: faker.string.numeric(4),
      }
    });
  }
  return createPixKeyPayload;
};

function generateMakePaymentPayload() {
  console.log(`Generating ${PAYMENT_PAYLOAD} makePayment payload...`);
  const VALID_ACCOUNT = JSON.parse(fs.readFileSync("./seed/valid_account.json"));
  const VALID_PIX_KEY = JSON.parse(fs.readFileSync("./seed/valid_pixKey.json"));
  const makePaymentPayload = [];
  for (let i = 0; i < PAYMENT_PAYLOAD; i++) {
    makePaymentPayload.push({
      origin: {
        user: {
          cpf: VALID_PIX_KEY[0].Value,
        },
        account: {
          number: VALID_ACCOUNT[0].Number,
          agency: VALID_ACCOUNT[0].Agency,
        },
      },
      destiny: {
        key: {
          value: VALID_PIX_KEY[0].Value,
          type: VALID_PIX_KEY[0].Type,
        },
      },
      amount: faker.number.int({ min: 1, max: 2147483646 }),
    });
  }
  return makePaymentPayload;
}

function generateJson(filepath, data) {
  if (fs.existsSync(filepath)) {
    fs.unlinkSync(filepath);
  }
  fs.writeFileSync(filepath, JSON.stringify(data));
}

function run() {
  const start = new Date();

  if (GENERATE_CREATE_PIX_KEY) {
    const pixKeyPayload = generateCreatePixKeyPayload();
    generateJson("./payload/payload_createPixKey.json", pixKeyPayload);
  }

  if (GENERATE_PAYMENT_PAYLOAD) {
    const makePaymentPayload = generateMakePaymentPayload();
    generateJson("./payload/payload_makePayment.json", makePaymentPayload);
  }

  const end = new Date();
  console.log("Done!");
  console.log(`Finished in ${(end - start) / 1000} seconds`);
  exit(0);
}

run();