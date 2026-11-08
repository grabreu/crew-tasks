# Resend for transactional email

Sign-in codes and notification emails are sent through Resend over SMTP (`smtp.resend.com`, port 465, user `resend`, the API key as password), using the existing MailKit sender. The sender is `no-reply@mail.grabreu.dev`: a dedicated subdomain, so the sending reputation stays apart from `grabreu.dev`, which hosts the portfolio. DNS is on Cloudflare (DKIM TXT, two CNAMEs for SPF and the return path, and a DMARC record with `p=none`), all **DNS only**. The API key is stored as the `email-password` Container App secret and passed as a secure parameter at Bicep deployment.

Considered and not chosen: Brevo (SMTP relay), Azure Communication Services Email (needs an Entra app and a client secret that expires, more setup for the same result), Cloudflare Email Service (in beta and tied to a paid plan when evaluated), and Gmail with an app password (fragile).

**Consequences**: switching provider is a configuration change, since only the SMTP settings differ. A new sending domain has no reputation, so early messages can land in spam. The provider's free plan has limits that apply to this project.
