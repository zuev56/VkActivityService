﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Xunit;

namespace IntegrationTests;

[ExcludeFromCodeCoverage]
public class ActivityLoggerTests : IDisposable
{
    private const int _dbEntitiesAmount = 100;

    public ActivityLoggerTests()
    {
        // Создание инстанса приложения
        //https://docs.microsoft.com/ru-ru/aspnet/core/test/integration-tests?view=aspnetcore-6.0
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(config =>
            {

            });

        var client = application.CreateClient();


        // Создание БД
        //https://stackoverflow.com/questions/60510597/integration-testing-with-asp-net-core-and-entity-framework-core-how-to-restore
        //DbContext = new MyDbContext(TestInit.TestDatabaseName);
        //DbContext.Database.CreateIfNotExists();
    }


    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLogger = GetActivityLogger();

        // Act
        var saveActivityResult = await activityLogger.SaveUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_Once_AddUndefinedStateToAll()
    {
        throw new NotImplementedException();

        // Arrange
        //var activityLoggerService = GetActivityLogger(_userIdSet);
        //var users = await _usersRepository!.FindAllAsync();
        //var before = await _activityLogItemsRepository!.FindLastUsersActivityAsync();
        //
        //// Act
        //var setUndefinedActivityResult = await activityLoggerService.SetUndefinedActivityToAllUsersAsync();
        //var after = await _activityLogItemsRepository.FindLastUsersActivityAsync();
        //after = after.OrderBy(i => i.Id).TakeLast(_dbEntitiesAmount).ToList(); // Because InMemory doesn't support RawSql
        //
        //// Assert
        //setUndefinedActivityResult.Should().NotBeNull();
        //setUndefinedActivityResult.IsSuccess.Should().BeTrue();
        //after.Should().HaveSameCount(users);
        //after.Should().OnlyContain(i => i.IsOnline == null);
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_ManyTimes_AddUndefinedActivityOnlyOnce()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_EmptyActivityLog_SuccessfulWithWarning()
    {
        throw new NotImplementedException();
    }


    private IActivityLogger GetActivityLogger()
    {


        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.GetFullPath(Constants.VkActivityServiceAppSettingsPath))
            .Build();

        var vkIntegration = new VkIntegration(configuration[AppSettings.Vk.AccessToken], configuration[AppSettings.Vk.Version]);

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.UsersRepository,
            vkIntegration,
            Mock.Of<ILogger<ActivityLogger>>(),
            Mock.Of<IDelayedLogger<ActivityLogger>>());
    }



    public void Dispose()
    {
        // Удаление БД
        //Database.Delete(TestDatabaseName);
    }
}
