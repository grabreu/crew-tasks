import { createFileRoute, Link, redirect } from "@tanstack/react-router";
import { authStore } from "./login";

const HomePage = () => {
  return (
    <div>
      <Link to="/logout">Log out</Link>
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
