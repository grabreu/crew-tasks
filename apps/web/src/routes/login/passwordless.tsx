import { revalidateLogic } from "@tanstack/react-form";
import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { PasswordlessCodeForm } from "~/features/auth/components/passwordless-code-form";
import { PasswordlessEmailForm } from "~/features/auth/components/passwordless-email-form";
import {
  passwordlessFormOptions,
  passwordlessFormSchema,
} from "~/features/auth/forms/passwordless-form";
import { useAppForm } from "~/lib/form";

const PasswordlessLoginPage = () => {
  const [step, setStep] = useState<"email" | "code">("email");

  const form = useAppForm({
    ...passwordlessFormOptions,
    validationLogic: revalidateLogic(),
    validators: { onDynamic: passwordlessFormSchema },
  });

  if (step === "email") {
    return (
      <PasswordlessEmailForm form={form} onEmailSent={() => setStep("code")} />
    );
  }

  return <PasswordlessCodeForm form={form} />;
};

export const Route = createFileRoute("/login/passwordless")({
  component: PasswordlessLoginPage,
});
