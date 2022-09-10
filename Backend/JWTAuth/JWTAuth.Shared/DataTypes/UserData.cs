using JWTAuth.Shared.DTOs;

namespace JWTAuth.Shared.DataTypes
{
    public class UserData
    {
        public UserDto UserDto { get; set; }
        public TokensData TokensData { get; set; }
    }
}
