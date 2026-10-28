import { mutationOptions } from "@tanstack/react-query";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const authMutations = {
  startPasswordlessLogin: () =>
    mutationOptions({
      mutationKey: ["auth", "login", "passwordless", "start"],
      mutationFn: async (input: { email: string }) => {
        await delay(1000);
        console.log(input);
        if (input.email === "error@shared-todo.dev") {
          throw new Error();
        }
      },
    }),

  completePasswordlessLogin: () =>
    mutationOptions({
      mutationKey: ["auth", "login", "passwordless", "complete"],
      mutationFn: async (input: { email: string; code: string }) => {
        await delay(1000);
        console.log(input);
        if (input.code === "000000") {
          throw new Error();
        }
        return { accessToken: "fake-token" };
      },
    }),
};
