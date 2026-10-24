import { useMutation } from "@tanstack/react-query";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { useState } from "react";
import z from "zod";
import { authMutations, authStore } from "~/features/auth";

const RouteComponent = () => {
  const [code, setCode] = useState("");

  const navigate = Route.useNavigate();
  const search = Route.useSearch();

  const mutation = useMutation({
    ...authMutations.completeEmailSignIn(),
    onSuccess: (data) => {
      authStore.actions.startSession(data.accessToken);
      navigate({ to: search.redirect || "/" });
    },
  });

  return (
    <div>
      <input
        type="text"
        placeholder="Code"
        value={code}
        onChange={(e) => setCode(e.target.value)}
      />
      <button
        type="button"
        onClick={() =>
          mutation.mutate({ email: "grabreu@shared-todo.dev", code })
        }
      >
        Complete Sign In with Email
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_auth/sign-in-complete")({
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
