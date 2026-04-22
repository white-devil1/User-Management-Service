using System.Security.Cryptography;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Services.Password;

public class PasswordGenerator : IPasswordGenerator
{
    public string GenerateTempPassword()
    {
        var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[RandomNumberGenerator.GetInt32(26)].ToString();
        var lowercase = "abcdefghijklmnopqrstuvwxyz"[RandomNumberGenerator.GetInt32(26)].ToString();
        var digit = "0123456789"[RandomNumberGenerator.GetInt32(10)].ToString();
        var special = "@#$!%^&*"[RandomNumberGenerator.GetInt32(8)].ToString();
        const string all =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$!%^&*";
        var remaining = new string(Enumerable.Repeat(all, 8)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        var chars = (uppercase + lowercase + digit + special + remaining).ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }
}
