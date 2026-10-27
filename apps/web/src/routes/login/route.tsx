import { createFileRoute, Outlet } from "@tanstack/react-router";

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
  component: LoginLayout,
});
