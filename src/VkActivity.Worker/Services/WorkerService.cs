﻿using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Data.Abstractions;
using VkActivity.Worker.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;
using static VkActivity.Worker.Models.Constants;

namespace VkActivity.Worker.Services;

// TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия
internal sealed class WorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScheduler _scheduler;
    private readonly IConnectionAnalyser _connectionAnalyser;
    private readonly IConfiguration _configuration;
    private readonly IDelayedLogger<WorkerService> _delayedLogger;
    private readonly ILogger<WorkerService> _logger;
    private DateTime _disconnectionTime = DateTime.UtcNow;

    private IJob _userActivityLoggerJob;
    private bool _isFirstStep = true;


    public WorkerService(
        IServiceScopeFactory serviceScopeFactory,
        IScheduler scheduler,
        IConnectionAnalyser connectionAnalyser,
        IConfiguration configuration,
        IDelayedLogger<WorkerService> delayedLogger,
        ILogger<WorkerService> logger)
    {
        try
        {
            _scopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _connectionAnalyser = connectionAnalyser ?? throw new ArgumentNullException(nameof(connectionAnalyser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _delayedLogger = delayedLogger ?? throw new ArgumentNullException(nameof(delayedLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _scheduler.Jobs.AddRange(CreateJobs());
        }
        catch (Exception ex)
        {
            var tiex = new TypeInitializationException(typeof(WorkerService).FullName, ex);
            _logger?.LogError(tiex, $"{nameof(WorkerService)} initialization error");
            throw;
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;
            _connectionAnalyser.Start(2000, 3000);

            _scheduler.Start(3000, 1000);

            _logger.LogInformation($"{nameof(WorkerService)} started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(WorkerService)} starting error");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _connectionAnalyser.ConnectionStatusChanged -= СonnectionAnalyser_StatusChanged;
        _connectionAnalyser.Stop();

        _scheduler.Stop();

        _logger.LogInformation($"{nameof(WorkerService)} stopped");
        return Task.CompletedTask;
    }

    private void СonnectionAnalyser_StatusChanged(ConnectionStatus status)
    {
        _disconnectionTime = status == ConnectionStatus.Ok
            ? default
            : DateTime.UtcNow;
    }

    private List<IJobBase> CreateJobs()
    {
        _userActivityLoggerJob = new ProgramJob(
            period: TimeSpan.FromSeconds(_configuration.GetSection(AppSettings.Vk.ActivityLogIntervalSec).Get<int>()),
            method: SaveVkUsersActivityAsync,
            description: nameof(_userActivityLoggerJob),
            logger: _logger);

        var userDataUpdaterJob = new ProgramJob(
            period: TimeSpan.FromHours(_configuration.GetSection(AppSettings.Vk.UsersDataUpdateIntervalHours).Get<int>()),
            startUtcDate: DateTime.UtcNow.Date.AddDays(1),
            method: UpdateUsersDataAsync,
            description: "userDataUpdaterJob",
            logger: _logger);

        return new List<IJobBase>()
        {
            _userActivityLoggerJob,
            userDataUpdaterJob
        };
    }

    /// <summary>Activity data collection</summary>
    private async Task SaveVkUsersActivityAsync()
    {
        if (_disconnectionTime != default)
        {
            await SetUndefinedActivityToAllUsers().ConfigureAwait(false);

            _delayedLogger.LogError(NoInernetConnection);
            return;
        }

        if (_isFirstStep)
        {
            _isFirstStep = false;
            _userActivityLoggerJob!.IdleStepsCount = 1;
            await AddUsersFullInfoAsync().ConfigureAwait(false);
            return;
        }

        if (_userActivityLoggerJob!.IdleStepsCount > 0)
            _userActivityLoggerJob.IdleStepsCount = 0;

        var result = await SaveUsersActivityAsync().ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            string logMessage = result.Messages.Count == 1
                ? result.Messages[0].Text
                : result.JoinMessages();

            _logger.LogErrorIfNeed(logMessage);
        }

        _logger.LogTraceIfNeed(result.JoinMessages());
    }

    private async Task<IOperationResult> SaveUsersActivityAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var activityLoggerService = scope.ServiceProvider.GetService<IActivityLogger>();
            return await activityLoggerService!.SaveUsersActivityAsync().ConfigureAwait(false);
        }
    }

    private async Task<IOperationResult> SetUndefinedActivityToAllUsers()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var activityLoggerService = scope.ServiceProvider.GetService<IActivityLogger>();
            return await activityLoggerService!.ChangeAllUserActivitiesToUndefinedAsync().ConfigureAwait(false);
        }
    }
    private async Task AddUsersFullInfoAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetService<IUserManager>();
            var initialUserIds = _configuration.GetSection(AppSettings.Vk.InitialUserIds).Get<string[]>();
            await userManager!.AddUsersAsync(initialUserIds ?? Array.Empty<string>()).ConfigureAwait(false);
        }
    }

    private async Task UpdateUsersDataAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersRepo = scope.ServiceProvider.GetService<IUsersRepository>();
            var userIds = await usersRepo!.FindAllIdsAsync().ConfigureAwait(false);

            var userManager = scope.ServiceProvider.GetService<IUserManager>()!;
            var updateResult = await userManager.UpdateUsersAsync(userIds);

            if (!updateResult.IsSuccess)
                _logger.LogError(string.Join(Environment.NewLine, updateResult.Messages), new { UserIds = userIds });
        }
    }
}