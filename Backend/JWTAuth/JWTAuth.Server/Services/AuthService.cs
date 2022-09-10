using JWTAuth.DataAccess.Models;
using JWTAuth.Shared.DataTypes;
using JWTAuth.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JWTAuth.Server.Services;

public class AuthService
{
    private readonly DbScope db;
    private readonly TokensService tokensService;
    private readonly ILogger<AuthService> logger;

    public AuthService(DbScope db, TokensService tokensService, ILogger<AuthService> logger)
    {
        this.db = db;
        this.tokensService = tokensService;
        this.logger = logger;
    }

    public async Task<UserData> RegistrationAsync(AuthDto request, UserAgentData userAgentData)
    {
        var user = await this.db.Users.FirstOrDefaultAsync(u => u.Email.Equals(request.Email));
        if (user is not null)
        {
            this.logger.LogError("Error register, user with such an email already exists!");
            throw new Exception("Не удалось зарегистрироваться, пользователь с таким Email уже существует!");
        }

        var hashPassword = Utilities.Hash(request.Password);

        this.logger.LogInformation("Add User data");

        var userEntry = await this.db.Users.AddAsync(new User
        {
            Email = request.Email,
            Password = hashPassword
        });

        await this.db.SaveChangesAsync();

        var addedUserId = userEntry.Entity.Id;
        var userDto = new UserDto { Id = addedUserId, Email = request.Email };

        this.logger.LogInformation("Generate tokens");
        var tokens = this.tokensService.GenerateTokens(userDto);

        this.logger.LogInformation("Save refresh token");
        await this.tokensService.SaveRefreshTokenAsync(addedUserId, tokens.RefreshJwt, userAgentData);

        var userData = new UserData { UserDto = userDto, TokensData = tokens };

        return userData;
    }

    public async Task<UserData> LoginAsync(AuthDto request, UserAgentData userAgentData)
    {
        var user = await this.db.Users.FirstOrDefaultAsync(u => u.Email.Equals(request.Email));
        if (user is null)
        {
            this.logger.LogError("User with such email is not found!");
            throw new Exception("Пользователь с таким Email не найден!");
        }

        if (!user.Password.Equals(Utilities.Hash(request.Password)))
        {
            this.logger.LogError("Invalid password!");
            throw new Exception("Неверный пароль!");
        }

        var userDto = new UserDto { Id = user.Id, Email = user.Email };

        this.logger.LogInformation("Generate tokens");
        var tokens = this.tokensService.GenerateTokens(userDto);

        this.logger.LogInformation("Save refresh token");
        await this.tokensService.SaveRefreshTokenAsync(user.Id, tokens.RefreshJwt, userAgentData);

        var userData = new UserData { UserDto = userDto, TokensData = tokens };

        return userData;
    }

    public async Task<string> LogoutAsync(string refreshToken)
    {
        this.logger.LogInformation("Remove refresh token");
        var removedRefreshToken = await this.tokensService.RemoveRefreshTokenAsync(refreshToken);
        return removedRefreshToken;
    }
}