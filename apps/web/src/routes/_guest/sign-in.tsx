import { useMutation } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { authMutations } from "~/features/auth/api";

const RouteComponent = () => {
  const [email, setEmail] = useState<string>("");
  const [formPhase, setFormPhase] = useState<"start" | "complete">("start");

  const navigate = useNavigate();

  const startMutation = useMutation({
    ...authMutations.startEmailSignIn(),
    onSuccess: (data) => {
      console.log("Sign-in started successfully:", data);
      setFormPhase("complete");
    },
    onError: (error) => {
      console.error("Error starting sign-in:", error);
    },
  });

  return (
    <div>
      <label htmlFor="email">Email:</label>
      <input
        id="email"
        type="text"
        placeholder="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <br />
      <button
        type="button"
        onClick={() => startMutation.mutate({ email: email })}
      >
        Continuar
      </button>
    </div>
  );
};

export const Route = createFileRoute("/_guest/sign-in")({
  component: RouteComponent,
});
