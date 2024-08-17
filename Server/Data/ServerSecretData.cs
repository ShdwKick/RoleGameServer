using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.Data
{
    public class ServerSecretData
    {
        private static string _hashSalt { get; set; } = "RoleGameHashSalt";
        private static string _baseUrl { get; set; } = "https://localhost";
        private static string _serverKey { get; set; } = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RoleGameSecretKey")).ToString();

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

        public static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input + _hashSalt); 
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2")); 
                }
                return builder.ToString();
            }
        }


        public static JwtSecurityToken GenerateNewToken(string userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServerSecretData.GetSecurityKey()));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub, userId),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var newToken = new JwtSecurityToken(
                issuer: "RoleGameServer",
                audience: "RoleGameClient",
                claims: claims,
                expires: DateTime.Now.AddMinutes(480),
                signingCredentials: credentials
            );
            return newToken;
        }
    }
}
