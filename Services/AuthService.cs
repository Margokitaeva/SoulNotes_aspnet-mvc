public static class AuthService
{
    public static bool Validate(string login, string token)
    {
        var user = DataBaseService.GetUserByLogin(login);
        return user != null && user.Token == token;
    }
}