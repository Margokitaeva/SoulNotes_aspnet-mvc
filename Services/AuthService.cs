using SoulNotes.Services;

public static class AuthService
{
    public static bool Validate(string login, string token)
    {
        var user = UserService.GetUserByLogin(login);
        return user != null && user.Token == token;
    }
}