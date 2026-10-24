import { useForm } from "@tanstack/react-form";
import { createFileRoute, Link } from "@tanstack/react-router";
import { CircleCheckBigIcon } from "lucide-react";
import { z } from "zod";
import { Button } from "~/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSeparator,
} from "~/components/ui/field";
import { Input } from "~/components/ui/input";

const GoogleIcon = () => {
  return (
    <svg viewBox="0 0 48 48" aria-hidden="true" data-icon="inline-start">
      <path
        fill="#FFC107"
        d="M43.611 20.083H42V20H24v8h11.303c-1.649 4.657-6.08 8-11.303 8-6.627 0-12-5.373-12-12s5.373-12 12-12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 12.955 4 4 12.955 4 24s8.955 20 20 20 20-8.955 20-20c0-1.341-.138-2.65-.389-3.917z"
      />
      <path
        fill="#FF3D00"
        d="M6.306 14.691l6.571 4.819C14.655 15.108 18.961 12 24 12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 16.318 4 9.656 8.337 6.306 14.691z"
      />
      <path
        fill="#4CAF50"
        d="M24 44c5.166 0 9.86-1.977 13.409-5.192l-6.19-5.238C29.211 35.091 26.715 36 24 36c-5.202 0-9.619-3.317-11.283-7.946l-6.522 5.025C9.505 39.556 16.227 44 24 44z"
      />
      <path
        fill="#1976D2"
        d="M43.611 20.083H42V20H24v8h11.303a12.04 12.04 0 0 1-4.087 5.571l.003-.002 6.19 5.238C36.971 39.205 44 34 44 24c0-1.341-.138-2.65-.389-3.917z"
      />
    </svg>
  );
};

const RouteComponent = () => {
  const form = useForm({
    defaultValues: {
      email: "",
    },
    validators: {
      onSubmit: z.object({
        email: z
          .email("Informe um e-mail válido")
          .max(256, "Informe um e-mail válido"),
      }),
    },
    onSubmit: ({ value }) => {
      alert(`Enviaríamos um código para ${value.email}`);
    },
  });

  return (
    <div className="flex min-h-svh flex-col items-center justify-center p-6 md:p-10">
      <main className="w-full max-w-90">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            form.handleSubmit();
          }}
        >
          <FieldGroup>
            <Field>
              <CircleCheckBigIcon className="size-10" />
            </Field>
            <Field className="text-center">
              <h1 className="text-balance text-2xl font-bold leading-tight">
                Tarefas de todo mundo, num só lugar.
              </h1>
              <p className="text-balance text-lg font-medium text-muted-foreground">
                Entre na sua conta
              </p>
            </Field>
            <form.Field name="email">
              {(field) => {
                const isInvalid =
                  field.state.meta.isTouched && !field.state.meta.isValid;
                return (
                  <Field data-invalid={isInvalid}>
                    <FieldLabel htmlFor={field.name}>E-mail</FieldLabel>
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
                      placeholder="nome@exemplo.com"
                    />
                    <FieldDescription>
                      Enviaremos um código para o seu e-mail. Sem senha.
                    </FieldDescription>
                    <FieldError errors={field.state.meta.errors} />
                  </Field>
                );
              }}
            </form.Field>
            <Field>
              <Button type="submit">Continuar</Button>
            </Field>
            <FieldSeparator>ou continue com</FieldSeparator>
            <Field>
              <Button
                variant="outline"
                type="button"
                onClick={() => alert("Entrar com Google ainda não está ligado")}
              >
                <GoogleIcon />
                Continuar com Google
              </Button>
            </Field>
            <FieldDescription className="text-center">
              Ao continuar, você aceita os <Link to="/">Termos de Uso</Link> e a{" "}
              <Link to="/">Política de Privacidade</Link>.
            </FieldDescription>
          </FieldGroup>
        </form>
      </main>
    </div>
  );
};

export const Route = createFileRoute("/")({
  component: RouteComponent,
});
