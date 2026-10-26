import { formOptions, revalidateLogic } from "@tanstack/react-form";
import { mutationOptions, useMutation } from "@tanstack/react-query";
import { createFileRoute, redirect, useNavigate } from "@tanstack/react-router";
import { createStore } from "@tanstack/react-store";
import { useState } from "react";
import { z } from "zod";
import { Button } from "~/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import { useAppForm, withForm } from "~/lib/form";

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

const loginMutations = {
  startPasswordlessLogin: mutationOptions({
    mutationFn: async (input: { email: string }) => console.log(input),
  }),
  completePasswordlessLogin: mutationOptions({
    mutationFn: async (input: { email: string; code: string }) => {
      console.log(input);
      return { accessToken: "fake-token" };
    },
  }),
};

const loginFormSchema = z.object({
  startPasswordlessLogin: z.object({
    email: z.email("Please enter a valid email address for login."),
  }),
  completePasswordlessLogin: z.object({
    code: z
      .string()
      .regex(/^\d{6}$/, "Please enter a valid verification code."),
  }),
});

const loginFormOpts = formOptions({
  defaultValues: {
    startPasswordlessLogin: { email: "" },
    completePasswordlessLogin: { code: "" },
  },
});

type LoginMethodSelectorProps = {
  onSelectPasswordless: () => void;
};

const LoginMethodSelector = ({
  onSelectPasswordless,
}: LoginMethodSelectorProps) => {
  return (
    <FieldSet>
      <FieldLegend className="mb-6 flex w-full flex-col items-center gap-4 text-center text-xl">
        <img src="/favicon-dark.svg" alt="Todos" className="size-10" />
        Log in to Todos
      </FieldLegend>
      <FieldGroup>
        <Field>
          <Button type="button" onClick={onSelectPasswordless}>
            Continue with email
          </Button>
          <Button type="button" variant="outline">
            Continue with Google
          </Button>
          <Button type="button" variant="outline">
            Continue with SAML SSO
          </Button>
          <Button type="button" variant="outline">
            Log in with passkey
          </Button>
        </Field>
      </FieldGroup>
    </FieldSet>
  );
};

const PasswordlessEmailForm = withForm({
  ...loginFormOpts,
  props: {
    onEmailSubmitted: () => {},
    onExit: () => {},
  },
  render: function Render({ form, onEmailSubmitted, onExit }) {
    const mutation = useMutation({
      ...loginMutations.startPasswordlessLogin,
      onSuccess: () => onEmailSubmitted(),
    });

    return (
      <form.FormGroup
        name="startPasswordlessLogin"
        onGroupSubmit={({ value }) => mutation.mutate(value)}
      >
        {(formGroup) => (
          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              formGroup.handleSubmit();
            }}
          >
            <FieldSet>
              <FieldLegend className="mb-6 flex w-full flex-col items-center gap-4 text-center text-xl">
                <img src="/favicon-dark.svg" alt="Todos" className="size-10" />
                What's your email address?
              </FieldLegend>
              <FieldGroup>
                <form.AppField name="startPasswordlessLogin.email">
                  {(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name} className="sr-only">
                          Email
                        </FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          type="email"
                          inputMode="email"
                          autoComplete="email"
                          placeholder="Enter your email address..."
                        />
                        <FieldError errors={field.state.meta.errors} />
                      </Field>
                    );
                  }}
                </form.AppField>
                <form.AppForm>
                  <Field>
                    <Button type="submit">Continue with email</Button>
                    <Button type="button" variant="ghost" onClick={onExit}>
                      Back to login
                    </Button>
                  </Field>
                </form.AppForm>
              </FieldGroup>
            </FieldSet>
          </form>
        )}
      </form.FormGroup>
    );
  },
});

const PasswordlessVerificationForm = withForm({
  ...loginFormOpts,
  props: {
    onExit: () => {},
  },
  render: function Render({ form, onExit }) {
    const navigate = useNavigate();

    const mutation = useMutation({
      ...loginMutations.completePasswordlessLogin,
      onSuccess: (data) => {
        authStore.actions.set(data.accessToken);
        navigate({ to: "/" });
      },
    });

    return (
      <form.FormGroup
        name="completePasswordlessLogin"
        onGroupSubmit={({ value }) =>
          mutation.mutate({
            email: form.state.values.startPasswordlessLogin.email,
            code: value.code,
          })
        }
      >
        {(formGroup) => (
          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              formGroup.handleSubmit();
            }}
          >
            <FieldSet>
              <FieldLegend className="mb-6 flex w-full flex-col items-center gap-4 text-center text-xl">
                <img src="/favicon-dark.svg" alt="Todos" className="size-10" />
                Check your email
              </FieldLegend>
              <FieldDescription className="mb-2 text-center">
                We've sent you a temporary login code. <br />
                Please check your inbox at <br />
                <form.Subscribe
                  selector={(state) =>
                    state.values.startPasswordlessLogin.email
                  }
                >
                  {(email) => <strong>{email}</strong>}
                </form.Subscribe>
              </FieldDescription>
              <FieldGroup>
                <form.AppField name="completePasswordlessLogin.code">
                  {(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name} className="sr-only">
                          Code
                        </FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          type="text"
                          inputMode="numeric"
                          autoComplete="one-time-code"
                          placeholder="Enter code"
                          maxLength={6}
                          className="text-center font-mono"
                        />
                        <FieldError errors={field.state.meta.errors} />
                      </Field>
                    );
                  }}
                </form.AppField>
                <form.AppForm>
                  <Field>
                    <Button type="submit">Continue with login code</Button>
                    <Button type="button" variant="ghost" onClick={onExit}>
                      Back to login
                    </Button>
                  </Field>
                </form.AppForm>
              </FieldGroup>
            </FieldSet>
          </form>
        )}
      </form.FormGroup>
    );
  },
});

type PasswordlessLoginFlowProps = {
  onExit: () => void;
};

const PasswordlessLoginFlow = ({ onExit }: PasswordlessLoginFlowProps) => {
  const [step, setStep] = useState(0);

  const form = useAppForm({
    ...loginFormOpts,
    validationLogic: revalidateLogic(),
    validators: {
      onDynamic: loginFormSchema,
    },
  });

  return (
    <>
      {step === 0 && (
        <PasswordlessEmailForm
          form={form}
          onEmailSubmitted={() => setStep((s) => s + 1)}
          onExit={onExit}
        />
      )}
      {step === 1 && (
        <PasswordlessVerificationForm form={form} onExit={onExit} />
      )}
    </>
  );
};

type LoginMethod = "passwordless" | null;

const LoginPage = () => {
  const [method, setMethod] = useState<LoginMethod>(null);

  return (
    <div className="flex min-h-svh flex-col items-center justify-center p-6 md:p-10">
      <main className="w-full max-w-90">
        {method === null && (
          <LoginMethodSelector
            onSelectPasswordless={() => setMethod("passwordless")}
          />
        )}
        {method === "passwordless" && (
          <PasswordlessLoginFlow onExit={() => setMethod(null)} />
        )}
      </main>
    </div>
  );
};

export const Route = createFileRoute("/login")({
  beforeLoad: () => {
    if (authStore.state.accessToken) {
      throw redirect({ to: "/" });
    }
  },
  component: LoginPage,
});
