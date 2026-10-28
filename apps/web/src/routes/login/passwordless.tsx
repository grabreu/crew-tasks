import { formOptions, revalidateLogic } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
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
import { authMutations } from "~/features/auth/mutations";
import { authStore } from "~/features/auth/store";
import { useAppForm, withForm } from "~/lib/form";

const passwordlessFormSchema = z.object({
  passwordlessEmail: z.object({
    email: z.email("Please enter a valid email address for login."),
  }),
  passwordlessVerification: z.object({
    code: z
      .string()
      .regex(/^\d{6}$/, "Please enter a valid verification code."),
  }),
});

const passwordlessFormOpts = formOptions({
  defaultValues: {
    passwordlessEmail: { email: "" },
    passwordlessVerification: { code: "" },
  },
});

const PasswordlessEmailForm = withForm({
  ...passwordlessFormOpts,
  props: {
    onNext: () => {},
  },
  render: function Render({ form, onNext }) {
    const mutation = useMutation({
      ...authMutations.startPasswordlessLogin(),
      onSuccess: () => onNext(),
    });

    return (
      <form.FormGroup
        name="passwordlessEmail"
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
                <form.AppField name="passwordlessEmail.email">
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
                          onChange={(e) => {
                            mutation.reset();
                            field.handleChange(e.target.value);
                          }}
                          aria-invalid={isInvalid}
                          type="email"
                          inputMode="email"
                          autoComplete="email"
                          placeholder="Enter your email address..."
                        />
                        <FieldError errors={field.state.meta.errors} />
                        {mutation.isError && (
                          <div
                            role="alert"
                            className="text-sm font-normal text-destructive"
                          >
                            Invalid email address. Please try again.
                          </div>
                        )}
                      </Field>
                    );
                  }}
                </form.AppField>
                <form.AppForm>
                  <Field>
                    <Button type="submit" disabled={mutation.isPending}>
                      Continue with email
                    </Button>
                    <Button
                      nativeButton={false}
                      variant="ghost"
                      render={<Link to="/login" />}
                    >
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
  ...passwordlessFormOpts,
  render: function Render({ form }) {
    const navigate = useNavigate();

    const mutation = useMutation({
      ...authMutations.completePasswordlessLogin(),
      onSuccess: (data) => {
        authStore.actions.set(data.accessToken);
        navigate({ to: "/" });
      },
    });

    return (
      <form.FormGroup
        name="passwordlessVerification"
        onGroupSubmit={({ value }) =>
          mutation.mutate({
            email: form.state.values.passwordlessEmail.email,
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
                  selector={(state) => state.values.passwordlessEmail.email}
                >
                  {(email) => <strong>{email}</strong>}
                </form.Subscribe>
              </FieldDescription>
              <FieldGroup>
                <form.AppField name="passwordlessVerification.code">
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
                          onChange={(e) => {
                            mutation.reset();
                            field.handleChange(e.target.value);
                          }}
                          aria-invalid={isInvalid}
                          type="text"
                          inputMode="numeric"
                          autoComplete="one-time-code"
                          placeholder="Enter code"
                          maxLength={6}
                          className="text-center font-mono"
                        />
                        <FieldError errors={field.state.meta.errors} />
                        {mutation.isError && (
                          <div
                            role="alert"
                            className="text-sm font-normal text-destructive"
                          >
                            Invalid verification code. Please try again.
                          </div>
                        )}
                      </Field>
                    );
                  }}
                </form.AppField>
                <form.AppForm>
                  <Field>
                    <Button type="submit" disabled={mutation.isPending}>
                      Continue with login code
                    </Button>
                    <Button
                      nativeButton={false}
                      variant="ghost"
                      render={<Link to="/login" />}
                    >
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

const steps = [PasswordlessEmailForm, PasswordlessVerificationForm];

const PasswordlessPage = () => {
  const [step, setStep] = useState(0);

  const form = useAppForm({
    ...passwordlessFormOpts,
    validationLogic: revalidateLogic(),
    validators: {
      onDynamic: passwordlessFormSchema,
    },
  });

  const Step = steps[step];

  return <Step form={form} onNext={() => setStep((prev) => prev + 1)} />;
};

export const Route = createFileRoute("/login/passwordless")({
  component: PasswordlessPage,
});
