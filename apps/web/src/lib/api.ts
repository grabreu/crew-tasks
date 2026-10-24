import aduana from "@grabreu/aduana";
import { authStore } from "~/features/auth";

export const api = aduana.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const accessToken = authStore.state.accessToken;

  return {
    ...config,
    headers: {
      ...config.headers,
      ...(accessToken && {
        Authorization: `Bearer ${accessToken}`,
      }),
    },
  };
});
