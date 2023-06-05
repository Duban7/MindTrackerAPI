using System.Security.Cryptography;

namespace BLL
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Генерируем соль
            byte[] salt;
            RandomNumberGenerator.Create().GetBytes(salt = new byte[16]);

            // Создаем объект класса Rfc2898DeriveBytes для хэширования пароля с использованием bcrypt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

            // Получаем хэш пароля
            byte[] hash = pbkdf2.GetBytes(20);

            // Комбинируем соль и хэш в один массив байтов
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Преобразуем массив байтов в строку
            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        public static bool VerifyPassword(string password, string savedPasswordHash)
        {
            // Преобразуем сохраненный хэш пароля из строки в массив байтов
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);

            // Извлекаем соль из массива байтов
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Создаем объект класса Rfc2898DeriveBytes с извлеченной солью
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

            // Вычисляем хэш для введенного пароля
            byte[] hash = pbkdf2.GetBytes(20);

            // Сравниваем вычисленный хэш с сохраненным хэшем
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false; // Пароль не совпадает
                }
            }

            return true; // Пароль совпадает
        }
    }
}