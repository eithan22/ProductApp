using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Helper
{
    public class PasswordHelper
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
