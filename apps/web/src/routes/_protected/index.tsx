import { useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { Button } from "~/components/ui/button";
import { useLogout } from "~/features/auth/hooks/use-logout";
import { userQueries } from "~/features/users/api/queries";

const HomePage = () => {
  const { data: currentUser } = useSuspenseQuery(userQueries.getCurrentUser());
  const logout = useLogout();

  return (
    <div>
      <h1>Welcome, {currentUser.email}!</h1>
      <Button type="button" variant="ghost" onClick={logout}>
        Log out
      </Button>
    </div>
  );
};

export const Route = createFileRoute("/_protected/")({
  component: HomePage,
});
