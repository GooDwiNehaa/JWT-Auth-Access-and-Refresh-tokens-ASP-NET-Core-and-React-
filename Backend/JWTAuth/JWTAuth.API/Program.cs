using JWTAuth.DataAccess.Models;
using JWTAuth.Server.BackgroundServices;
using JWTAuth.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Services

            builder.Logging.AddFile();

            builder.Services.AddCors();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<DbScope>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);

            builder.Services.AddSingleton<TokensService>();

            builder.Services.AddScoped<UsersService>();
            builder.Services.AddScoped<AuthService>();

            builder.Services.AddHostedService<ExpiredTokenCleaner>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
                   {
                       options.RequireHttpsMetadata = true;
                       options.TokenValidationParameters = TokensService.GetTokenValidationParameters(true);
                   });

            builder.Services.AddAuthorization();

            builder.Services.AddCookiePolicy(options =>
            {
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });

            builder.Services
                   .AddControllers()
                   .AddNewtonsoftJson();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #endregion

            var app = builder.Build();

            #region Middlewares

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var clientUrl = app.Configuration.GetSection("URLS").GetChildren().First(c => c.Key.Equals("ClientUrl")).Value;

            app.UseRouting();

            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseCors(b => b.WithOrigins(clientUrl)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials()
                              .Build());

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            #endregion

            app.Run();
        }
    }
}