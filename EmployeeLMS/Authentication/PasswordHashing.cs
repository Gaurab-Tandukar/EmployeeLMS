using BCrypt.Net;

namespace EmployeeLMS.Authentication
{
    public class PasswordHashing
    {
        // hashing
        public static String PasswordHash(String plainPassword)
        {

            string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            return hash;
        }

        // verifying 
        public static bool VerifyPassword(String plainPassword, String hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
    }
}
