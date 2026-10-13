import aduana from "@grabreu/aduana";

export const api = aduana.create({
  baseURL: import.meta.env.VITE_API_URL,
});
