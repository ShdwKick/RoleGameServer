using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Server.Data
{
    public class ServerSecretData
    {
        //TODO: перенести в конгфиг файл
        private static string _hashSalt { get; set; } = "RoleGameHashSalt";
        private static string _baseUrl { get; set; } = "https://localhost";

        private static string _serverKey { get; set; } =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RoleGameSecretKey")).ToString();

        private static string _issuer = "RoleGameServer";
        private static string _audience = "RoleGameClient";
        
        public static string GetSalt()
        {
            return _hashSalt;
        }

        public static string GetSecurityKey()
        {
            return _serverKey;
        }

        public static string GetBaseUrl()
        {
            return _baseUrl;
        }

        public static string GetIssuer()
        {
            return _issuer;
        }

        public static string GetAudience()
        {
            return _audience;
        }
    }
}