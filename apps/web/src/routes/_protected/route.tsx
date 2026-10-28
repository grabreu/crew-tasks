import { createFileRoute, redirect } from "@tanstack/react-router";
import { authStore } from "~/features/auth/store/auth-store";
import { userQueries } from "~/features/users/api/queries";

export const Route = createFileRoute("/_protected")({
  beforeLoad: () => {
    if (!authStore.state.accessToken) {
      throw redirect({
        to: "/login",
      });
    }
  },
  loader: ({ context }) => {
    return context.queryClient.query(userQueries.getCurrentUser());
  },
});
