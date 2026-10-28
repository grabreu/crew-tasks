import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { authStore } from "~/features/auth/store/auth-store";

export const useLogout = () => {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const logout = () => {
    queryClient.clear();
    authStore.actions.clear();
    navigate({ to: "/login" });
  };

  return logout;
};
