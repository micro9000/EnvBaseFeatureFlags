using Gama.Core.FeatureFlag.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App;
public static class FeatureFlagRegistration
{
    public static void AddFeatureFlagService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<List<Flag>>()
            .Bind(configuration.GetSection(FeatureFlagOptions.FeatureFlags));

        services.AddOptions<List<FeatureFlagUserGroup>>()
            .Bind(configuration.GetSection(FeatureFlagOptions.FeatureFlagUserGroups));

        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
    }
}
