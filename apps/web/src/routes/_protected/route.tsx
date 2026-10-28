import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import { authStore } from "~/features/auth/store";
import { userQueries } from "~/features/users/queries";

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
  component: Outlet,
});
