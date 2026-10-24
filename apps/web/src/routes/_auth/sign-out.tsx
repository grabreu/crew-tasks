import { createFileRoute, redirect } from "@tanstack/react-router";
import { authStore } from "~/features/auth";

export const Route = createFileRoute("/_auth/sign-out")({
  preload: false,
  beforeLoad: () => {
    if (!authStore.state.accessToken) {
      throw redirect({
        to: "/sign-in",
      });
    }
  },
  loader: async ({ context }) => {
    context.queryClient.clear();
    authStore.actions.endSession();
    throw redirect({ to: "/sign-in" });
  },
});
