using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BonesCoreOrm.Generators.Intefaces;
using stORM.utils;
using static stORM.Models.GroupByModel;

namespace BonesCoreOrm.Generators;

public sealed class DeleteGen(Config config) : IGenerator
{
    private Config _config = config;

    public string Generate(dynamic MainEntity)
    {
        try
        {
            _config.ConfigMainEntity();
            GenerateDeleteWhere(MainEntity);
            var script = _config.Script;
            _config = _config.DeriveNew();

            return script;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GenerateDeleteWhere(dynamic MainEntity) =>
    _config.Script += $" DELETE {_config.MainTable} {CodesEnum.BR} WHERE {$"  {CodesEnum.BR} " +
        $"{GetPrimaryKey()} = {GetPrimaryKeyValue(MainEntity)} "}";

    private string GetPrimaryKey() =>
        _config.MainEntity
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.Name ??
                throw new Exception($"Primary Key from {_config.MainEntity.Name} was not found!");

    private string GetPrimaryKeyValue(dynamic MainEntity)
    {
        var value = _config.MainEntity
                .GetProperties().ToList()
                .FindAll(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)
                .FirstOrDefault(prop => UtilsService.IsNotNull(prop.GetValue(MainEntity, null)))?.GetValue(MainEntity, null)
                ?? throw new Exception($"Primary Key value from {_config.MainEntity.Name} was not found!");

        if (value.GetType() == typeof(Guid) || value.GetType() == typeof(string))
            return $"'{value.ToString()}'";
        else
            return value.ToString();
    }
}