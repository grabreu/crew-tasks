import { useMutation } from "@tanstack/react-query";
import { Link, useNavigate } from "@tanstack/react-router";
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
import { authMutations } from "~/features/auth/api/mutations";
import { passwordlessFormOptions } from "~/features/auth/forms/passwordless-form";
import { authStore } from "~/features/auth/store/auth-store";
import { withForm } from "~/lib/form";

export const PasswordlessCodeForm = withForm({
  ...passwordlessFormOptions,
  render: function Render({ form }) {
    const navigate = useNavigate();

    const completeLogin = useMutation({
      ...authMutations.completePasswordlessLogin(),
      onSuccess: ({ accessToken }) => {
        authStore.actions.setAccessToken(accessToken);
        navigate({ to: "/" });
      },
    });

    return (
      <form.FormGroup
        name="codeStep"
        onGroupSubmit={({ value }) =>
          completeLogin.mutate({
            email: form.state.values.emailStep.email,
            code: value.code,
          })
        }
      >
        {(group) => (
          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              group.handleSubmit();
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
                  selector={(state) => state.values.emailStep.email}
                >
                  {(email) => <strong>{email}</strong>}
                </form.Subscribe>
              </FieldDescription>
              <FieldGroup>
                <form.AppField name="codeStep.code">
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
                            completeLogin.reset();
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
                        {completeLogin.isError && (
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
                    <Button type="submit" disabled={completeLogin.isPending}>
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
