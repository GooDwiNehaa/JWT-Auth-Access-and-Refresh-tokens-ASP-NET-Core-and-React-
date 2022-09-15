using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using JWTAuth.DataAccess.Models;
using JWTAuth.Shared.DataTypes;
using JWTAuth.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuth.Server.Services
{
    public class TokensService
    {
        private readonly DbScope db;
        private readonly ILogger<TokensService> logger;

        public TokensService(DbScope db, ILogger<TokensService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        #region Public Methods

        public async Task SaveRefreshTokenAsync(
            Guid userId,
            string refreshToken,
            UserAgentData userAgentData)
        {
            var userToken = await this.db.Tokens
                                      .Include(t => t.UserAgent)
                                      .FirstOrDefaultAsync(t => t.UserID.Equals(userId)
                                                                && t.UserAgent.OS.Equals(userAgentData.OS)
                                                                && t.UserAgent.Browser.Equals(userAgentData.Browser));

            if (userToken is not null)
            {
                var now = DateTime.UtcNow;

                userToken.RefreshToken = refreshToken;
                userToken.Created = now;
                userToken.Expired = now.AddMinutes(userToken.LifeTime);

                await this.db.SaveChangesAsync();
            }
            else
            {
                var transaction = await this.db.Database.BeginTransactionAsync();

                var token = await this.db.Tokens.AddAsync(new Token { UserID = userId, RefreshToken = refreshToken });
                await this.db.UsersAgent.AddAsync(new UserAgent { Id = token.Entity.Id, OS = userAgentData.OS, Browser = userAgentData.Browser });

                await this.db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public TokensData GenerateTokens(UserDto user)
        {
            this.logger.LogInformation("Generate refresh token");
            var refreshToken = this.GenerateRefreshToken();

            this.logger.LogInformation("Generate access token");
            var accessJwt = this.GenerateAccessToken(user);

            return new TokensData { AccessJwt = accessJwt, RefreshJwt = refreshToken };
        }

        public async Task<string> RemoveRefreshTokenAsync(string refreshToken)
        {
            var token = await this.db.Tokens.FirstOrDefaultAsync(t => t.RefreshToken.Equals(refreshToken));
            if (token == null)
            {
                this.logger.LogError("In database refresh token is not found!");
                throw new Exception("Не удалось найти рефреш-токен в базе данных!");
            }

            this.db.Tokens.Remove(token);
            await this.db.SaveChangesAsync();

            return refreshToken;
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var tokens = await this.db.Tokens.Where(t => DateTime.UtcNow >= t.Expired).ToListAsync();
            if (tokens.Any())
            {
                this.db.Tokens.RemoveRange(tokens);
                await this.db.SaveChangesAsync();
            }
        }

        public async Task<UserData?> RefreshAsync(
            TokensData tokens,
            UserAgentData userAgentData)
        {
            if (!await this.ValidateAccessTokenAsync(tokens.AccessJwt))
            {
                this.logger.LogError("Invalid access token!");
                throw new Exception("Невалидный токен доступа!");
            }

            if (!await this.ValidateRefreshTokenAsync(tokens.RefreshJwt))
            {
                this.logger.LogError("Invalid refresh token!");
                throw new Exception("Невалидный рефреш токен!");
            }

            var user = await this.db.Tokens
                                 .Include(t => t.UserAgent)
                                 .Where(t => t.RefreshToken.Equals(tokens.RefreshJwt)
                                             && t.UserAgent.OS.Equals(userAgentData.OS)
                                             && t.UserAgent.Browser.Equals(userAgentData.Browser))
                                 .Select(t => t.User)
                                 .FirstOrDefaultAsync();

            if (user is null)
            {
                this.logger.LogError("In database refresh token is not found!");
                throw new Exception("Рефреш токен не найден в базе данных!");
            }

            var userDto = new UserDto { Id = user.Id, Email = user.Email };

            this.logger.LogInformation("Generate tokens");
            var tokensDto = this.GenerateTokens(userDto);

            return new UserData { UserDto = userDto, TokensData = tokensDto };
        }

        public static TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = false) =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = AccessTokenOptions.ISSUER,

            ValidateAudience = true,
            ValidAudience = AccessTokenOptions.AUDIENCE,

            ClockSkew = TimeSpan.Zero,

            ValidateLifetime = validateLifetime,
            IssuerSigningKey = AccessTokenOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };

        #endregion

        #region Private Methods

        private string GenerateAccessToken(UserDto user)
        {
            var now = DateTime.UtcNow;
            var expires = now.Add(TimeSpan.FromSeconds(AccessTokenOptions.LIFETIME));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email)
            };

            var jwt = new JwtSecurityToken(AccessTokenOptions.ISSUER,
                AccessTokenOptions.AUDIENCE,
                claims,
                expires: expires,
                signingCredentials: new SigningCredentials(AccessTokenOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<bool> ValidateAccessTokenAsync(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claimsPrincipal = tokenHandler.ValidateToken(accessToken, GetTokenValidationParameters(), out var _);

            var claims = claimsPrincipal.Claims.ToList();

            var userIdClaim = claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier));
            if (userIdClaim is null)
            {
                return false;
            }

            var userEmailClaim = claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email));
            if (userEmailClaim is null)
            {
                return false;
            }

            var userExist = await this.db.Users.AnyAsync(u => u.Id.Equals(new Guid(userIdClaim.Value)) && u.Email.Equals(userEmailClaim.Value));
            if (!userExist)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await this.db.Tokens.FirstOrDefaultAsync(t => t.RefreshToken.Equals(refreshToken));
            if (token is null)
            {
                return false;
            }

            if (DateTime.UtcNow >= token.Expired)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
