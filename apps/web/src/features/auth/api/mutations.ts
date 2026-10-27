import { mutationOptions } from "@tanstack/react-query";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const authMutations = {
  startPasswordlessLogin: () =>
    mutationOptions({
      mutationFn: async (input: { email: string }) => {
        await delay(1000);
        console.log(input);
      },
    }),

  completePasswordlessLogin: () =>
    mutationOptions({
      mutationFn: async (input: { email: string; code: string }) => {
        await delay(1000);
        console.log(input);
        return { accessToken: "fake-token" };
      },
    }),
};
