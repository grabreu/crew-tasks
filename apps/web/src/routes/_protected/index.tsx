import { useQueryClient, useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { Button } from "~/components/ui/button";
import { authStore } from "~/features/auth/store";
import { userQueries } from "~/features/users/queries";

const HomePage = () => {
  const currentUserQuery = useSuspenseQuery(userQueries.getCurrentUser());
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const logout = () => {
    queryClient.clear();
    authStore.actions.clear();
    navigate({ to: "/login" });
  };

  return (
    <div>
      <h1>Welcome, {currentUserQuery.data.email}!</h1>
      <Button type="button" variant="ghost" onClick={logout}>
        Log out
      </Button>
    </div>
  );
};

export const Route = createFileRoute("/_protected/")({
  component: HomePage,
});
