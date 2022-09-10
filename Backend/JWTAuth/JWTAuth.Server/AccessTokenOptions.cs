using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuth.Server
{
    public class AccessTokenOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "MyAuthClient"; // потребитель токена
        private const string KEY = "mysupersecret_secretkey!123"; // ключ для шифрации
        public const int LIFETIME = 30; // время жизни токена - 30 секунд

        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}