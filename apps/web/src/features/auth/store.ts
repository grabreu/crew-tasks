import { createStore } from "@tanstack/react-store";

type AuthState = {
  accessToken: string | null;
};

type AuthActions = {
  set: (accessToken: string) => void;
  clear: () => void;
};

const initialState: AuthState = {
  accessToken: null,
};

export const authStore = createStore<AuthState, AuthActions>(
  initialState,
  ({ setState }) => ({
    set: (accessToken: string) =>
      setState(() => ({
        accessToken,
      })),
    clear: () => setState(() => initialState),
  }),
);
