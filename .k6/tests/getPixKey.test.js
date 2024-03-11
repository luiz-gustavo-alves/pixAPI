import http from "k6/http";
import { SharedArray } from "k6/data";

export const options = {
  vus: 30,
  duration: "20s",
};

const pixKeys = new SharedArray("PixKeys", function () {
  const result = JSON.parse(open("../seed/existing_pixKeys.json"));
  return result;
});

const token = new SharedArray("Token", function () {
  const result = JSON.parse(open("../seed/existing_token.json"));
  return result;
});

export default function () {
  const pixKey = pixKeys[Math.floor(Math.random() * pixKeys.length)];
  const headers = { "Content-Type": "application/json", "Authorization": `Bearer ${token[0].token}` };
  http.get(`${__ENV.BASE_URL}/keys/${pixKey.Type}/${pixKey.Value}`, { headers });
}