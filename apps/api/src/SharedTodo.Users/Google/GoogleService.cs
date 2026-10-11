namespace SharedTodo.Users.Google;

public class GoogleService(IOptions<GoogleOptions> options) : IGoogleService
{
    public async Task<GoogleIdentity?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [options.Value.ClientId]
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            return payload.EmailVerified ? new GoogleIdentity(payload.Subject, payload.Email) : null;
        }
        catch (Exception exception) when (exception is InvalidJwtException or Newtonsoft.Json.JsonException)
        {
            return null;
        }
    }
}
