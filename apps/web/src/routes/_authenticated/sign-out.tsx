import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/sign-out")({
  beforeLoad: () => {
    localStorage.removeItem("access_token");
    throw redirect({ to: "/sign-in" });
  },
});
