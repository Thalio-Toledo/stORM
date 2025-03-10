using BonesCore.BonesCoreOrm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BonesCore.Extensions;

public static class stOrmExtensions
{
    public static IServiceCollection AddstORM(this IServiceCollection services, IConfiguration configuration) => services.AddTransient<stORMCore>();
}