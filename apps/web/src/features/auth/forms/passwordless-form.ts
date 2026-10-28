import { formOptions } from "@tanstack/react-form";
import { z } from "zod";

export const passwordlessFormSchema = z.object({
  emailStep: z.object({
    email: z.email("Please enter a valid email address for login."),
  }),
  codeStep: z.object({
    code: z
      .string()
      .regex(/^\d{6}$/, "Please enter a valid verification code."),
  }),
});

export const passwordlessFormOptions = formOptions({
  defaultValues: {
    emailStep: { email: "" },
    codeStep: { code: "" },
  },
});
