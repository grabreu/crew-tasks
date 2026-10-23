import { useMutation } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { authMutations } from "~/features/auth/api";

const RouteComponent = () => {
  const navigate = useNavigate();

  const mutation = useMutation({
    ...authMutations.completeEmailSignIn(),
    onSuccess: (data) => {
      console.log("Sign-in completed successfully:", data);
      localStorage.setItem("access_token", data.accessToken);
      navigate({ to: "/" });
    },
    onError: (error) => {
      console.error("Error completing sign-in:", error);
    },
  });

  return (
    <div>
      <input type="text" placeholder="Email" />
      <input type="text" placeholder="Code" />
      <button
        type="button"
        onClick={() =>
          mutation.mutate({ email: "grabreu@shared-todo.dev", code: "123456" })
        }
      >
        Complete Sign In with Email
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_guest/sign-in/complete")({
  component: RouteComponent,
});
