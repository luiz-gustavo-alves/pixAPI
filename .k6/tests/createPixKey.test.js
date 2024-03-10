import http from "k6/http";
import { SharedArray } from "k6/data";

export const options = {
  vus: 30,
  duration: "20s",
};

const users = new SharedArray("Users", function () {
  const result = JSON.parse(open("../seed/existing_users.json"));
  return result;
});

const payloads = new SharedArray("Payload", function () {
  const result = JSON.parse(open("../payload/existing_createPixKeyPayload.json"));
  return result;
})

const token = new SharedArray("Token", function () {
  const result = JSON.parse(open("../seed/existing_token.json"));
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

  const headers = { "Content-Type": "application/json", "Authorization": `Bearer ${token[0].token}` };
 http.post(`http://localhost:5180/keys`, JSON.stringify(body), { headers });
}