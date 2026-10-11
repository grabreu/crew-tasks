namespace SharedTodo.Users.Identity;

public static class IdentityResultExtensions
{
    public static ErrorList ToErrors(this IdentityResult identity)
    {
        var errors = identity.Errors.Select(error => Result.Invalid(error.Code, error.Description)).ToList();
        return new ErrorList(errors.Count > 0 ? errors : [Result.Failure("Identity operation failed.")]);
    }
}
