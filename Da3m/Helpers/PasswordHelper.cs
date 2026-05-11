
// Da3m/Helpers/PasswordHelper.cs
namespace Da3m.Helpers
{
    public class PasswordHelper
    {

            // ✅ تشفير كلمة المرور
            public static string Hash(string password)
                => BCrypt.Net.BCrypt.HashPassword(password);

            // ✅ التحقق من كلمة المرور
            public static bool Verify(
                string password, string hashedPassword)
                => BCrypt.Net.BCrypt.Verify(
                    password, hashedPassword);

            // ✅ هل كلمة المرور مشفّرة أصلاً؟
            public static bool IsHashed(string password)
                => password.StartsWith("$2a$") ||
                   password.StartsWith("$2b$");
        
    }
}

