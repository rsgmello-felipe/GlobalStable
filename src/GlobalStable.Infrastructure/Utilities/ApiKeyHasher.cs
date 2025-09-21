namespace GlobalStable.Infrastructure.Utilities;

public static class ApiKeyHasher
{
    public static string Sha256(string input, string pepper)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input + pepper);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
