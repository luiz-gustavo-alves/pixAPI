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

const pixKeys = new SharedArray("PixKeys", function () {
  const result = JSON.parse(open("../seed/existing_pixKeys.json"));
  return result;
});

const token = new SharedArray("Token", function () {
  const result = JSON.parse(open("../seed/valid_bank.json"));
  return result;
});

export default function () {
  const pixKey = pixKeys[Math.floor(Math.random() * pixKeys.length)];
  const headers = { "Content-Type": "application/json", "Authorization": `Bearer ${token[0].Token}` };
  http.get(`${__ENV.BASE_URL}/keys/${pixKey.Type}/${pixKey.Value}`, { headers });
}