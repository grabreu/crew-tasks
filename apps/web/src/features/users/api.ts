import { queryOptions } from "@tanstack/react-query";
import { api } from "~/lib/api";

export const userQueries = {
  all: () => ["users"] as const,
  getCurrentUser: () =>
    queryOptions({
      queryKey: [...userQueries.all(), "current"],
      queryFn: () =>
        api
          .get<{
            id: string;
            email: string;
            displayName: string;
            isOnboarded: boolean;
          }>("/users/me")
          .then((response) => response.data),
    }),
};
