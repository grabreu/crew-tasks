import { createFileRoute, redirect } from "@tanstack/react-router";
import { authStore } from "./login";

export const Route = createFileRoute("/logout")({
  preload: false,
  beforeLoad: () => {
    if (!authStore.state.accessToken) {
      throw redirect({
        to: "/login",
      });
    }
  },
  loader: async ({ context }) => {
    context.queryClient.clear();
    authStore.actions.clear();
    throw redirect({ to: "/login" });
  },
});
