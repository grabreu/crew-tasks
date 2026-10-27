import { createFileRoute, Link } from "@tanstack/react-router";
import { Button } from "~/components/ui/button";
import {
  Field,
  FieldGroup,
  FieldLegend,
  FieldSet,
} from "~/components/ui/field";

const LoginPage = () => {
  return (
    <FieldSet>
      <FieldLegend className="mb-6 flex w-full flex-col items-center gap-4 text-center text-xl">
        <img src="/favicon-dark.svg" alt="Todos" className="size-10" />
        Log in to Todos
      </FieldLegend>
      <FieldGroup>
        <Field>
          <Button
            nativeButton={false}
            render={<Link to="/login/passwordless" />}
          >
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

export const Route = createFileRoute("/login/")({
  component: LoginPage,
});
