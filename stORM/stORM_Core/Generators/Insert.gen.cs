using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BonesCoreOrm.Generators.Intefaces;
using stORM.utils;
using static stORM.Models.GroupByModel;

namespace BonesCoreOrm.Generators;

public sealed class InsertGen(Config config) : IGenerator
{
    private Config _config = config;
    public string Generate(dynamic entity)
    {
        _config.ConfigMainEntity();
        SetColumnsValueCreate(entity);
        GenerateInsertOutput();
        GenerateInsertInto();
        GenerateInsertOutputValue();
        GenerateInsertValues();
        GenerateInsertReturn();

        var script = _config.Script;
        _config = _config.DeriveNew();

        return script;
    }

    private bool IsIntegerType(Type type) =>
               type == typeof(int) ||
               type == typeof(long) ||
               type == typeof(short) ||
               type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(ushort) ||
               type == typeof(uint) ||
               type == typeof(ulong);

    private bool IsGuidType(Type type) => type == typeof(Guid);

    private void GenerateInsertOutput() => _config.Script += $"{CodesEnum.BR}DECLARE @OUTPUT TABLE({GetPrimaryKey()} {GetOutputType()})";

    private void GenerateInsertInto() =>
        _config.Script += $"{CodesEnum.BRDOUBLE}INSERT INTO {_config.MainTable}(" +
         $"{CodesEnum.BRTAB}{string.Join(",", _config.ColumnsSet.ConvertAll(itemColumns => { return itemColumns.Name; }))}" +
         $"{CodesEnum.BR})";

    private void GenerateInsertOutputValue() => _config.Script += $"{CodesEnum.BR}OUTPUT INSERTED.{GetPrimaryKey()} INTO @OUTPUT";

    private void GenerateInsertReturn() => _config.Script += $"{CodesEnum.BRDOUBLE}SELECT {GetPrimaryKey()} FROM @OUTPUT";

    private void GenerateInsertValues() =>
        _config.Script +=
            $"{CodesEnum.BR}VALUES(" +
            $"{CodesEnum.BRTAB}{string.Join(",", _config.ColumnsSet.ConvertAll(itemColumns => { return itemColumns.GetValue(); }))}" +
            $"{CodesEnum.BR})";

    protected void SetColumnsValueCreate(dynamic MainEntity) =>
         _config.MainEntity
                .GetProperties()
                .ToList()
                .FindAll(prop => !prop.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .FindAll(prop => !(prop.PropertyType.IsClass && prop.PropertyType != typeof(string)))
                .FindAll(prop => UtilsService.IsNotNull(prop.GetValue(MainEntity, null)))
                .ForEach(prop => _config.ColumnsSet.Add(new ColumnSet(prop.Name, prop.GetValue(MainEntity, null))));
    private string GetPrimaryKey() =>
       _config.MainEntity
               .GetProperties()
               .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.Name ??
               throw new Exception($"Primary Key from {_config.MainEntity.Name} was not found!");
    private Type GetPrimaryKeyType() =>
    _config.MainEntity
            .GetProperties()
            .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.PropertyType ??
            throw new Exception($"Primary Key from {_config.MainEntity.Name} was not found!");

    private string GetOutputType()
    {
        if (IsIntegerType(GetPrimaryKeyType())) return "bigint";
        else if (IsGuidType(GetPrimaryKeyType())) return "UNIQUEIDENTIFIER";
        else return "";
    }
}
