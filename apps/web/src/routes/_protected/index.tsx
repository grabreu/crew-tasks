import { useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { userQueries } from "~/features/users";

const RouteComponent = () => {
  const navigate = useNavigate();
  const currentUserQuery = useSuspenseQuery(userQueries.getCurrentUser());

  return (
    <div>
      <h1>Welcome, {currentUserQuery.data.email}!</h1>
      <button type="button" onClick={() => navigate({ to: "/sign-out" })}>
        Sign Out
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_protected/")({
  component: RouteComponent,
});
