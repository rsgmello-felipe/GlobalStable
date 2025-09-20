namespace GlobalStable.Domain.Constants;

public static class UserIdentifiers
{
    public const string ClientId = "client_id";
    public const string Username = "username";

    public static readonly HashSet<string> FullSet = [ClientId, Username];
}