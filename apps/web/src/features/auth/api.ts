import { mutationOptions } from "@tanstack/react-query";
import { api } from "~/lib/api";

export const authMutations = {
  startEmailSignIn: () =>
    mutationOptions({
      mutationFn: (input: { email: string }) =>
        api.post("/auth/email/sign-in", input),
    }),
  completeEmailSignIn: () =>
    mutationOptions({
      mutationFn: (input: { email: string; code: string }) =>
        api
          .post<{ accessToken: string }>("/auth/email/sign-in/complete", input)
          .then((response) => response.data),
    }),
};
