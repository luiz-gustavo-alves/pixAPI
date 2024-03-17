import http from "k6/http";
import { SharedArray } from "k6/data";

export const options = {
  vus: 10,
  duration: "20s",
};

const user = new SharedArray("Users", function () {
  const result = JSON.parse(open("../seed/valid_user.json"));
  return result;
});

const payloads = new SharedArray("Payload", function () {
  const result = JSON.parse(open("../payload/payload_makePayment.json"));
  return result;
})

const token = new SharedArray("Token", function () {
  const result = JSON.parse(open("../seed/valid_bank.json"));
  return result;
});

export default function () {
  const payload = payloads[Math.floor(Math.random() * payloads.length)];
  const body = {
    origin: {
      user: {
        cpf: user[0].CPF,
      },
      account: payload.origin.account,
    },
    destiny: payload.destiny,
    amount: payload.amount,
  };

  const headers = { "Content-Type": "application/json", "Authorization": `Bearer ${token[0].Token}` };
  http.post(`${__ENV.BASE_URL}/payments`, JSON.stringify(body), { headers });
}