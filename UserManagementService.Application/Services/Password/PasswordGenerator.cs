using UserManagementService.Application.Services;

namespace UserManagementService.Application.Services.Password;

public class PasswordGenerator : IPasswordGenerator
{
    public string GenerateTempPassword()
    {
        var random = new Random();
        var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)].ToString();
        var lowercase = "abcdefghijklmnopqrstuvwxyz"[random.Next(26)].ToString();
        var digit = "0123456789"[random.Next(10)].ToString();
        var special = "@#$!%^&*"[random.Next(8)].ToString();
        const string all =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$!%^&*";
        var remaining = new string(Enumerable.Repeat(all, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        var chars = (uppercase + lowercase + digit + special + remaining).ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }
}
