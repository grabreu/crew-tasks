import { mutationOptions } from "@tanstack/react-query";

export const authMutations = {
  startPasswordlessLogin: () =>
    mutationOptions({
      mutationKey: ["auth", "login", "passwordless", "start"],
      mutationFn: async (input: { email: string }) => {
        // TODO: mock failure trigger, remove with the real API call
        if (input.email === "error@shared-todo.dev") {
          throw new Error();
        }
      },
    }),

  completePasswordlessLogin: () =>
    mutationOptions({
      mutationKey: ["auth", "login", "passwordless", "complete"],
      mutationFn: async (input: { email: string; code: string }) => {
        // TODO: mock failure trigger, remove with the real API call
        if (input.code === "000000") {
          throw new Error();
        }
        return { accessToken: "fake-token" };
      },
    }),
};
