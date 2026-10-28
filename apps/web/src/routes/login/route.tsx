import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import { authStore } from "~/features/auth/store";

const LoginLayout = () => {
  return (
    <div className="flex min-h-svh flex-col items-center justify-center p-6 md:p-10">
      <main className="w-full max-w-90">
        <Outlet />
      </main>
    </div>
  );
};

export const Route = createFileRoute("/login")({
  beforeLoad: () => {
    if (authStore.state.accessToken) {
      throw redirect({ to: "/" });
    }
  },
  component: LoginLayout,
});
