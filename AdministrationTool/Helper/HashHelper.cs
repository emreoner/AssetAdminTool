using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdministrationTool.Helper
{
    public static class HashHelper
    {
        private static readonly Lazy<Random> Random = new Lazy<Random>(() => new Random((int)DateTime.Now.Ticks));

        public static KeyValuePair<string, string> Hash(string password, string salt, int count)
        {

            if (string.IsNullOrEmpty(salt))
                salt = RandomSalt(64);

            string tempHash;

            using (var sha = SHA256.Create())
            {
                tempHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
                for (var i = 1; i < count; i++)
                {
                    tempHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(tempHash + salt)));
                }
            }

            return new KeyValuePair<string, string>(salt, tempHash);
        }

        private static string RandomSalt(int size)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var abc = Convert.ToChar(Random.Value.Next() & char.MaxValue);
                builder.Append(abc);
            }

            string hashedSalt;

            using (var sha = SHA256.Create())
            {
                hashedSalt = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())));
            }

            return hashedSalt;
        }
    }
}
