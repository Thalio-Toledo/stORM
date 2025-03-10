using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BonesCoreOrm.Generators.Intefaces;
using stORM.utils;
using static stORM.Models.GroupByModel;

namespace BonesCoreOrm.Generators;

public sealed class UpdateGen(Config config) : IGenerator
{
    private Config _config = config;

    public string Generate(dynamic MainEntity)
    {
        try
        {
            _config.ConfigMainEntity();
            SetColumnsValueUpdate(MainEntity);
            GenerateUpdateSet(MainEntity);
            GenerateUpdateWhere(MainEntity);
            var script = _config.Script;
            _config = _config.DeriveNew();

            return script;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void GenerateUpdateSet(dynamic MainEntity) =>
        _config.Script += $"UPDATE " +
        $"{_config.MainTable} SET " +
        $"{string.Join(",", _config.ColumnsSet.ConvertAll(columnMap => { return $"{CodesEnum.BRTAB} {columnMap.Name} = {columnMap.GetValue()}"; }))}";

    protected void SetColumnsValueUpdate(dynamic MainEntity) =>
         _config.MainEntity
                .GetProperties()
                .ToList()
                .FindAll(prop => !prop.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .FindAll(prop => !(prop.PropertyType.IsClass && prop.PropertyType != typeof(string)))
                .FindAll(prop => UtilsService.IsNotNull(prop.GetValue(MainEntity, null)))
                .ForEach(prop => _config.ColumnsSet.Add(new ColumnSet(prop.Name, prop.GetValue(MainEntity, null))));

    protected void GenerateUpdateWhere(dynamic MainEntity) =>
        _config.Script += $"{CodesEnum.BR}WHERE {$"  {CodesEnum.BR} " +
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