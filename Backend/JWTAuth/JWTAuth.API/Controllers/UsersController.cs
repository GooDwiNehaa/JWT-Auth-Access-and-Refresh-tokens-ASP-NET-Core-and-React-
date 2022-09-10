using JWTAuth.Server.Services;
using JWTAuth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService userService;
        private readonly ILogger<UsersController> logger;

        /// <inheritdoc />
        public UsersController(UsersService userService, ILogger<UsersController> logger)
        {
            this.userService = userService;
            this.logger = logger;
        }

        [Authorize]
        [HttpGet(nameof(GetUsers))]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            try
            {
                this.logger.LogInformation("Start GetUsers");
                var users = await this.userService.GetAllUsersAsync();
                if (!users.Any())
                {
                    this.logger.LogWarning("Пользователи отсутствуют в базе данных!");
                }

                return this.Ok(users);
            }
            catch (Exception e)
            {
                this.logger.LogError($"Failed GetUsers:{e}");
                return this.BadRequest($"Ошибка при получении списка всех пользователей:{e.Message}");
            }
        }
    }
}
