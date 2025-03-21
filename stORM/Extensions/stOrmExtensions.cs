using BonesCore.BonesCoreOrm;
using BonesCore.ConfigOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BonesCore.ConfigOptions;
using stORM.stORM_Core;

namespace stORM.Extensions;

public static class stOrmExtensions
{
    public static IServiceCollection AddstORM(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection configOptions = configuration.GetSection("ConnectionStrings");
        services.Configure<DBConnectionOptions>(configOptions);

        services.AddTransient<stORMCore>();

        return services;
    }
}