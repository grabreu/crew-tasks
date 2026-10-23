import { createFileRoute, useNavigate } from "@tanstack/react-router";

const RouteComponent = () => {
  const navigate = useNavigate();

  return (
    <div>
      <button type="button" onClick={() => navigate({ to: "/sign-out" })}>
        Sign Out
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_authenticated/")({
  component: RouteComponent,
});
