import { useMutation } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Button } from "~/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import { authMutations } from "~/features/auth/api/mutations";
import { passwordlessFormOptions } from "~/features/auth/forms/passwordless-form";
import { withForm } from "~/lib/form";

export const PasswordlessEmailForm = withForm({
  ...passwordlessFormOptions,
  props: {
    onEmailSent: () => {},
  },
  render: function Render({ form, onEmailSent }) {
    const startLogin = useMutation({
      ...authMutations.startPasswordlessLogin(),
      onSuccess: onEmailSent,
    });

    return (
      <form.FormGroup
        name="emailStep"
        onGroupSubmit={({ value }) => startLogin.mutate(value)}
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
                What's your email address?
              </FieldLegend>
              <FieldGroup>
                <form.AppField name="emailStep.email">
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
                            startLogin.reset();
                            field.handleChange(e.target.value);
                          }}
                          aria-invalid={isInvalid}
                          type="email"
                          inputMode="email"
                          autoComplete="email"
                          placeholder="Enter your email address..."
                        />
                        <FieldError errors={field.state.meta.errors} />
                        {startLogin.isError && (
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
                    <Button type="submit" disabled={startLogin.isPending}>
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
