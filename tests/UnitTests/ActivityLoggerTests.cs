using System.Linq;
using System.Threading.Tasks;
using UnitTests.Data;
using Xunit;

namespace UnitTests;

public class ActivityLoggerTests
{
    private const int _dbEntitiesAmount = 1000;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(_dbEntitiesAmount);

    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = StubFactory.GetActivityLoggerService(_userIdSet);

        // Act
        var saveActivityResult = await activityLoggerService.SaveVkUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_VkIntegrationFailed_ReturnsError()
    {
        // Arrange
        var activityLoggerService = StubFactory.GetActivityLoggerService(_userIdSet, vkIntergationWorks: false);

        // Act
        var saveActivityResult = await activityLoggerService.SaveVkUsersActivityAsync();

        // Assert
        Assert.False(saveActivityResult?.IsSuccess);
        Assert.NotEmpty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }
}