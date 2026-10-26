import { createFileRoute, redirect, useNavigate } from "@tanstack/react-router";
import { authStore } from "./login";

const HomePage = () => {
  const navigate = useNavigate();

  return (
    <div>
      <button type="button" onClick={() => navigate({ to: "/logout" })}>
        Log out
      </button>
    </div>
  );
};

export const Route = createFileRoute("/")({
  beforeLoad: () => {
    if (!authStore.state.accessToken) {
      throw redirect({
        to: "/login",
      });
    }
  },
  component: HomePage,
});
