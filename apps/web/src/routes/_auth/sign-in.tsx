import { useMutation } from "@tanstack/react-query";
import { createFileRoute, redirect } from "@tanstack/react-router";
import z from "zod";
import { authMutations, authStore } from "~/features/auth";

const RouteComponent = () => {
  const navigate = Route.useNavigate();
  const search = Route.useSearch();

  const mutation = useMutation({
    ...authMutations.startEmailSignIn(),
    onSuccess: () => {
      navigate({
        to: "/sign-in-complete",
        search: { redirect: search.redirect },
      });
    },
  });

  return (
    <div>
      <button
        type="button"
        onClick={() => mutation.mutate({ email: "grabreu@shared-todo.dev" })}
      >
        Start Sign In with Email
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_auth/sign-in")({
  validateSearch: z.object({
    redirect: z.string().optional().catch(""),
  }),
  beforeLoad: ({ search }) => {
    if (authStore.state.accessToken) {
      throw redirect({ to: search.redirect || "/" });
    }
  },
  component: RouteComponent,
});
