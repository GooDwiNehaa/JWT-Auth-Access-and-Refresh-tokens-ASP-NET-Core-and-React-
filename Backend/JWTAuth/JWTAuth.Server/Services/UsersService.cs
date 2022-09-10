using JWTAuth.DataAccess.Models;
using JWTAuth.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JWTAuth.Server.Services;

public class UsersService
{
    private readonly DbScope db;
    private readonly ILogger<UsersService> logger;

    public UsersService(DbScope db, ILogger<UsersService> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await this.db.Users.Select(u => u).ToListAsync();

        var usersList = new List<UserDto>();

        foreach (var user in users)
        {
            usersList.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email
            });
        }

        return usersList;
    }
}