import { mutationOptions } from "@tanstack/react-query";
import { createStore } from "@tanstack/react-store";
import { api } from "~/lib/api";

export const authMutations = {
  startEmailSignIn: () =>
    mutationOptions({
      mutationFn: (input: { email: string }) =>
        api.post("/auth/email/sign-in", input),
    }),
  completeEmailSignIn: () =>
    mutationOptions({
      mutationFn: (input: { email: string; code: string }) =>
        api
          .post<{ accessToken: string }>("/auth/email/sign-in/complete", input)
          .then((response) => response.data),
    }),
};

export type AuthState = {
  accessToken: string | null;
  isAuthenticated: boolean;
};

export type AuthActions = {
  startSession: (accessToken: string) => void;
  endSession: () => void;
};

export const initialState: AuthState = {
  accessToken: null,
  isAuthenticated: false,
};

export const authStore = createStore<AuthState, AuthActions>(
  initialState,
  ({ setState }) => ({
    startSession: (accessToken: string) =>
      setState(() => ({
        accessToken,
        isAuthenticated: true,
      })),
    endSession: () => setState(() => initialState),
  }),
);
