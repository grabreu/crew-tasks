import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import { authStore } from "~/features/auth";
import { userQueries } from "~/features/users";

export const Route = createFileRoute("/_protected")({
  beforeLoad: ({ location }) => {
    if (!authStore.state.accessToken) {
      throw redirect({
        to: "/sign-in",
        search: {
          redirect: location.href,
        },
      });
    }
  },
  loader: ({ context }) => {
    return context.queryClient.query(userQueries.getCurrentUser());
  },
  component: Outlet,
});
