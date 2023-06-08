using System.Security.Cryptography;

namespace BLL
{
    public static class Hasher
    {
        public static string Hash(string origin)
        {
            byte[] salt;
            RandomNumberGenerator.Create().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(origin, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string originHash = Convert.ToBase64String(hashBytes);

            return originHash;
        }

        public static bool Verify(string stringToVerify, string originHash)
        {
            byte[] hashBytes = Convert.FromBase64String(originHash);
            byte[] salt = new byte[16];

            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(stringToVerify, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false; 

            return true; 
        }
    }
}