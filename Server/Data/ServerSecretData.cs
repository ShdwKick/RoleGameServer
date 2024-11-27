using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Server.Data
{
    public class ServerSecretData
    {
        private static string _hashSalt;
        private static string _baseUrl;
        private static string _serverKey;
        private static string _issuer;
        private static string _audience;


        public ServerSecretData(IConfiguration config)
        {
            _hashSalt = config["AppSettings:HashSalt"];
            _baseUrl = config["AppSettings:BaseUrl"];
            _serverKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["AppSettings:ServerKey"])).ToString();
            _issuer = config["AppSettings:Issuer"];
            _audience = config["AppSettings:Audience"];
        }

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
