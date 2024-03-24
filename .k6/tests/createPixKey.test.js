import http from "k6/http";
import { SharedArray } from "k6/data";

export const options = {
  scenarios: {
    spike_usage: {
      executor: "constant-arrival-rate",
      duration: "60s",
      preAllocatedVUs: 30,
      maxVUs: 30,
      rate: 50,
      timeUnit: "1s",
    },
  },
};

const users = new SharedArray("Users", function () {
  const result = JSON.parse(open("../seed/existing_users.json"));
  return result;
});

const payloads = new SharedArray("Payload", function () {
  const result = JSON.parse(open("../payload/payload_createPixKey.json"));
  return result;
})

const token = new SharedArray("Token", function () {
  const result = JSON.parse(open("../seed/valid_bank.json"));
  return result;
});

export default function () {
  const user = users[Math.floor(Math.random() * users.length)];
  const payload = payloads[Math.floor(Math.random() * payloads.length)];
  const body = {
    key: payload.key,
    user: {
      cpf: user.CPF
    },
    account: payload.account,
  };

  const headers = { "Content-Type": "application/json", "Authorization": `Bearer ${token[0].Token}` };
  http.post(`${__ENV.BASE_URL}/keys`, JSON.stringify(body), { headers });
}