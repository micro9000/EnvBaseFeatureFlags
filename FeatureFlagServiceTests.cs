using Microsoft.Extensions.Options;
using Xunit;

public class FeatureFlagServiceTests
{

    [Fact]
    public void GetUsersFeatureFlags_ReturnsCorrectFlags()
    {
        // Arrange
        var now = DateTime.Now;
        var userUpn = "user@example.com";

        var flags = new List<Flag>
        {
            new Flag
            {
                Id = "flag1",
                Filter = null // Should be added
            },
            new Flag
            {
                Id = "flag2",
                Filter = new FeatureFlagFilter
                {
                    Users = new List<string> { userUpn },
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(-1),
                        End = now.AddDays(1)
                    }
                }
            },
            new Flag
            {
                Id = "flag3",
                Filter = new FeatureFlagFilter
                {
                    Users = new List<string>(), // Empty user list
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(-2),
                        End = null
                    }
                }
            },
            new Flag
            {
                Id = "flag4",
                Filter = new FeatureFlagFilter
                {
                    Users = new List<string> { "another@example.com" }, // User not included
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(1), // Start in future
                        End = now.AddDays(2)
                    }
                }
            }
        };

        var flagsOptions = Options.Create(flags);
        var featureFlagUserGroupsOptions = Options.Create(new List<FeatureFlagUserGroup>());
        var service = new FeatureFlagService(flagsOptions, featureFlagUserGroupsOptions);

        // Act
        var result = service.GetUsersFeatureFlags(userUpn);

        // Assert
        Assert.Collection(result,
            item => Assert.Equal("flag1", item.id),
            item => Assert.Equal("flag2", item.id),
            item => Assert.Equal("flag3", item.id)
        );

        Assert.Equal(3, result.Count);
    }


    [Fact]
    public void GetUsersFeatureFlags_WithUserGroups_ReturnsCorrectFlags()
    {
        // Arrange
        var now = DateTime.Now;
        var userUpn = "user@example.com";
        var developerGroup = "developer";
        var supportGroup = "support";

        var flags = new List<Flag>
        {
            new Flag
            {
                Id = "flag1",
                Filter = null // Should be added
            },
            new Flag
            {
                Id = "flag2",
                Filter = new FeatureFlagFilter
                {
                    UserGroups = [developerGroup],
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(-1),
                        End = now.AddDays(1)
                    }
                }
            },
            new Flag
            {
                Id = "flag3",
                Filter = new FeatureFlagFilter
                {
                    UserGroups = [developerGroup],
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(-2),
                        End = null
                    }
                }
            },
            new Flag
            {
                Id = "flag4",
                Filter = new FeatureFlagFilter
                {
                    UserGroups = [supportGroup],
                    Users = new List<string> { "another1@example.com" }, // User not included
                    Timeline = new FeatureFlagTimeline
                    {
                        Start = now.AddDays(1), // Start in future
                        End = now.AddDays(2)
                    }
                }
            }
        };

        var groups = new List<FeatureFlagUserGroup>
        {
            new FeatureFlagUserGroup{Id = developerGroup, Users = [userUpn]},
            new FeatureFlagUserGroup{Id = supportGroup, Users = ["another2@example.com"]}
        };

        var flagsOptions = Options.Create(flags);
        var featureFlagUserGroupsOptions = Options.Create(new List<FeatureFlagUserGroup>());
        var service = new FeatureFlagService(flagsOptions, featureFlagUserGroupsOptions);

        // Act
        var result = service.GetUsersFeatureFlags(userUpn);

        // Assert
        Assert.Collection(result,
            item => Assert.Equal("flag1", item.id),
            item => Assert.Equal("flag2", item.id),
            item => Assert.Equal("flag3", item.id)
        );

        Assert.Equal(3, result.Count);
    }
}
