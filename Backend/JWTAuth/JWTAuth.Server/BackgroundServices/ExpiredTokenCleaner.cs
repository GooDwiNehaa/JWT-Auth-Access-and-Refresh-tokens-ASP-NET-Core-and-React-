using JWTAuth.Server.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JWTAuth.Server.BackgroundServices;

public class ExpiredTokenCleaner : BackgroundService
{
    private readonly TokensService tokensService;
    private readonly ILogger<ExpiredTokenCleaner> logger;
    private Timer timer;

    /// <inheritdoc />
    public ExpiredTokenCleaner(TokensService tokensService, ILogger<ExpiredTokenCleaner> logger)
    {
        this.tokensService = tokensService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Start BackgroundService ExpiredTokenCleaner");

        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startMessage = "Start executing RemoveExpiredTokensAsync";
        var executeMessage = "Executing RemoveExpiredTokensAsync";

        this.logger.LogInformation(startMessage);
        await Console.Out.WriteLineAsync(startMessage);
        this.timer = new Timer(async _ =>
            {
                this.logger.LogInformation(executeMessage);
                await Console.Out.WriteLineAsync(executeMessage);
                await this.tokensService.RemoveExpiredTokensAsync();
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(3));
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Stop BackgroundService ExpiredTokenCleaner");

        await base.StopAsync(cancellationToken);

        await this.timer.DisposeAsync();
    }
}