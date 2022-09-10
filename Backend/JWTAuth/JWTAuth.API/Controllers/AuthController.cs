using JWTAuth.DataAccess.Models;
using JWTAuth.Server;
using JWTAuth.Server.Services;
using JWTAuth.Shared.DataTypes;
using JWTAuth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokensService tokensService;
        private readonly AuthService authService;
        private readonly ILogger<AuthController> logger;
        private readonly string refreshTokenKey;

        /// <inheritdoc />
        public AuthController(
            TokensService tokensService,
            AuthService authService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            this.tokensService = tokensService;
            this.authService = authService;
            this.logger = logger;
            this.refreshTokenKey = configuration.GetSection("CookiesNames").GetChildren().First(c => c.Key.Equals("RefreshToken")).Value;
        }

        [AllowAnonymous]
        [HttpPost(nameof(Registration))]
        public async Task<ActionResult<UserData>> Registration([FromBody] AuthDto request)
        {
            try
            {
                var userAgentData = Utilities.GetUserAgentData(this.Request.Headers["User-Agent"]);

                this.logger.LogInformation("Start registration");
                var userData = await this.authService.RegistrationAsync(request, userAgentData);

                this.logger.LogInformation("Add refresh token cookie");
                this.AddRefreshTokenCookie(new Token(), userData);

                return this.Ok(userData);
            }
            catch (Exception e)
            {
                return this.BadRequest($"Ошибка при регистрации:{e.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost(nameof(Login))]
        public async Task<ActionResult<UserData>> Login([FromBody] AuthDto request)
        {
            try
            {
                var userAgentData = Utilities.GetUserAgentData(this.Request.Headers["User-Agent"]);

                this.logger.LogInformation("Start login");
                var userData = await this.authService.LoginAsync(request, userAgentData);

                this.logger.LogInformation("Add refresh token cookie");
                this.AddRefreshTokenCookie(new Token(), userData);

                return this.Ok(userData);
            }
            catch (Exception e)
            {
                return this.BadRequest($"Ошибка при выходе в аккаунт:{e.Message}");
            }
        }

        [Authorize]
        [HttpDelete(nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var cookieExist = this.Request.Cookies.TryGetValue(this.refreshTokenKey, out var refreshTokenValue);
                if (!cookieExist)
                {
                    this.logger.LogError("Refresh token cookie is not found!");
                    throw new Exception("Куки рефреш токена отсутствуют!");
                }

                this.logger.LogInformation("Start logout");
                await this.authService.LogoutAsync(refreshTokenValue!);

                this.logger.LogInformation("Delete Refresh token cookie");
                this.Response.Cookies.Delete(this.refreshTokenKey);

                return this.Ok();
            }
            catch
            {
                return this.BadRequest("Ошибка при выходе из аккаунта!");
            }
        }

        [AllowAnonymous]
        [HttpPut(nameof(Refresh))]
        public async Task<ActionResult<UserData>> Refresh([FromBody] string accessToken)
        {
            try
            {
                var cookieIsExist = this.Request.Cookies.TryGetValue(this.refreshTokenKey, out var refreshTokenValue);
                if (!cookieIsExist)
                {
                    this.logger.LogError("Refresh token cookie is not found!");
                    throw new Exception("Куки рефреш токена отсутствуют!");
                }

                var userAgentData = Utilities.GetUserAgentData(this.Request.Headers["User-Agent"]);

                var tokens = new TokensData { AccessJwt = accessToken, RefreshJwt = refreshTokenValue! };

                this.logger.LogInformation("Start refresh");
                var userData = await this.tokensService.RefreshAsync(tokens, userAgentData);

                this.logger.LogInformation("Save refresh token");
                await this.tokensService.SaveRefreshTokenAsync(userData!.UserDto.Id, userData.TokensData.RefreshJwt, userAgentData);

                this.logger.LogInformation("Add refresh token cookie");
                this.AddRefreshTokenCookie(new Token(), userData);

                return this.Ok(userData);
            }
            catch
            {
                return this.Unauthorized("Невозможно авторизоваться!");
            }
        }

        private void AddRefreshTokenCookie(Token refreshToken, UserData userData)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = refreshToken.Expired,
                MaxAge = TimeSpan.FromMinutes(refreshToken.LifeTime)
            };

            this.Response.Cookies.Append(this.refreshTokenKey, userData.TokensData.RefreshJwt, cookieOptions);
        }
    }
}
