import { queryOptions } from "@tanstack/react-query";

export const userQueries = {
  all: () => ["users"] as const,
  getCurrentUser: () =>
    queryOptions({
      queryKey: [...userQueries.all(), "current"],
      queryFn: async () => {
        return {
          id: "fake-id",
          email: "fake-email",
          displayName: "fake-display-name",
          isOnboarded: false,
        };
      },
    }),
};
