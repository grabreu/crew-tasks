import { queryOptions } from "@tanstack/react-query";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const userQueries = {
  all: () => ["users"] as const,
  getCurrentUser: () =>
    queryOptions({
      queryKey: [...userQueries.all(), "current"],
      queryFn: async () => {
        await delay(1000);
        return {
          id: "fake-id",
          email: "fake-email",
          displayName: "fake-display-name",
          isOnboarded: false,
        };
      },
    }),
};
