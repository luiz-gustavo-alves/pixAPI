const fs = require("fs");
const { faker } = require("@faker-js/faker");

const CREATE_PIX_KEY_PAYLOAD = 100000;

const GENERATE_CREATE_PIX_KEY = true;

function generateCreatePixKeyPayload() {
  console.log(`Generating ${CREATE_PIX_KEY_PAYLOAD} createPixKeys payload...`);
  const createPixKeyPayload = [];
  for (let i = 0; i < CREATE_PIX_KEY_PAYLOAD; i++) {
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

function generateJson(filepath, data) {
  if (fs.existsSync(filepath)) {
    fs.unlinkSync(filepath);
  }
  fs.writeFileSync(filepath, JSON.stringify(data));
}

function run() {
  const start = new Date();

  if (GENERATE_CREATE_PIX_KEY) {
    const createPixKeyPayload = generateCreatePixKeyPayload();
    generateJson("./payload/existing_createPixKeyPayload.json", createPixKeyPayload);
  }

  const end = new Date();
  console.log("Done!");
  console.log(`Finished in ${(end - start) / 1000} seconds`);
}

run();