using BonesCore.ConfigOptions;
using BonesCoreOrm.Generators;
using BonesCoreOrm.Generators.Intefaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using stORM.Models;
using static stORM.Models.GroupByModel;

namespace BonesCore.BonesCoreOrm;

public class stORMCore
{
    private static DBConnectionOptions _connectionOptions;
    public Config? _config;
    private IServiceProvider _serviceProvider;

    public stORMCore()
    {
        
    }
    public stORMCore(IOptions<DBConnectionOptions> options, IServiceProvider provider)
    {
        _connectionOptions = options.Value;
        _serviceProvider = provider;
    }

    internal void SetConfig(Config config)
    {
        _config = config;
    }

    public dynamic Generate<Generator>(dynamic entity = null)
        where Generator : IGenerator
    {
        var generator = ActivatorUtilities.CreateInstance<Generator>(_serviceProvider, _config);
        try
        {
            var script = generator.Generate(entity);
            GenerateScriptDebug(script);

            if (generator is SelectGen) return _connectionOptions.Query(script);
            if (generator is InsertGen) return _connectionOptions.Insert(script);
            return _connectionOptions.Execute(script);
        }
        catch (Exception ex) 
        {
            Rollback();
            return null;
        }

    }
    public async Task<string> GenerateAsync<Generator>(dynamic entity = null)
     where Generator : IGenerator
    {
        var generator = ActivatorUtilities.CreateInstance<Generator>(_serviceProvider, _config);
        var script = string.Empty;
        try
        {
            script = generator.Generate(entity);

            GenerateScriptDebug(script);

        }
        catch (Exception ex) 
        {
            Rollback();
        }
       
        return await _connectionOptions.QueryAsync(script);

    }

    public void GenerateScriptDebug(string script) =>
        Console.WriteLine(script
            .Replace(BaseScripts.JSON_START, "")
            .Replace(BaseScripts.JSON_END, ""));

    public string Query(string query, object parameters) => _connectionOptions.Query(query, parameters);
    public IEnumerable<T> Query<T>(string query, object parameters) => _connectionOptions.Query<T>(query, parameters);
    public string QueryFirst(string query, object parameters) => _connectionOptions.QueryFirst(query, parameters);
    public static void Rollback() => _connectionOptions.Rollback();
    public static void Savechanges() => _connectionOptions.Commit();
    public void RollBack() => _connectionOptions.Rollback();
    public void SaveChanges() => _connectionOptions.Commit();
    
}
