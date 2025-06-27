using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using BonesCoreOrm.Generators.Intefaces;
using stORM.DataAnotattions;
using stORM.Models;
using stORM.utils;
using static stORM.Models.GroupByModel;

namespace BonesCoreOrm.Generators;

public class SelectGen(Config config) : IGenerator
{
    private Config _config = config;

    public virtual string Generate(dynamic entity)
    {
        try
        {
            _config.Script += BaseScripts.JSON_START;
            SetMainEntityColumns();
            GenerateMainSelect();
            GenerateJoins();
            GenerateWhere();
            GenerateOrderBy();
            GeneratePaginacao();
            _config.Script += BaseScripts.JSON_END;

            var script = _config.Script;
            _config = _config.DeriveNew();

            return script;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    protected virtual void SetMainEntityColumns()
    {
        _config.ConfigMainEntity();

        var primitiveProperties = new List<PropertyInfo>();
        var entitiesProperties = new List<PropertyInfo>();

        void SplitProperties()
        {
            foreach (PropertyInfo mainTableProp in _config.MainEntity.GetProperties())
            {
                if (mainTableProp.PropertyType.IsClass && mainTableProp.PropertyType != typeof(string))
                {
                    if (mainTableProp.PropertyType.IsGenericType && mainTableProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = mainTableProp.PropertyType.GetGenericArguments()[0];
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == _config.MainEntity.Name))
                            entitiesProperties.Add(mainTableProp);
                    }
                    else
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == mainTableProp.PropertyType.Name && joinExist.Name == mainTableProp.Name))
                        entitiesProperties.Add(mainTableProp);
                }
                else
                    primitiveProperties.Add(mainTableProp);
            }
        }
        SplitProperties();

        if (_config.isCount is false)
            primitiveProperties.ForEach(primitiveProperty => SetMainEntityColumnsDefaultNoAlias(primitiveProperty.Name));


        foreach (PropertyInfo entity in entitiesProperties)
        {
            var EntitiesList = new List<string>();
            EntitiesList.Add(entity.Name);

            _config.MainPropName = entity.Name;

            var fullInheritance = new List<string>();
            fullInheritance.Add(entity.Name);

            if (entity.PropertyType.IsGenericType && entity.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = entity.PropertyType.GetGenericArguments()[0];

                var join = _config.JoinsList.Find(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == _config.MainEntity.Name);

                var entityColumn = _config.EntitiesColumns.Find(column => column.EntityName == _config.MainEntity.Name);
                if (entityColumn is null)
                {
                    entityColumn = new EntityColumns() { EntityName = _config.MainEntity.Name };
                    entityColumn.Columns.Add(SetColumnsOneToMany(itemType, _config.MainEntity, entity.Name, _config.MainAlias, false, join: join, fullInheritance));
                    _config.EntitiesColumns.Add(entityColumn);
                }

                entityColumn.Columns.Add(SetColumnsOneToMany(itemType, _config.MainEntity, entity.Name, _config.MainAlias, join: join));

                if (_config.ExistsList.Exists(ExistModel => ExistModel.Entity == itemType.Name && ExistModel.MainEntity == _config.MainEntity.Name))
                {
                    _config.WhereList.Add(SetColumnsOneToMany(itemType, _config.MainEntity, entity.Name, _config.MainAlias, isExists: true));
                }
            }
            else
                SetColumnsOneToOne(entity, _config.MainEntity, entity.Name, _config.MainAlias, false, fullInheritance: EntitiesList);
        }
    }

    private string GetForeignKey(PropertyInfo daughterEntity, Type parentEntity) =>
        parentEntity
            .GetProperties()
            .FirstOrDefault(prop => prop.GetCustomAttribute<ForeignkeyFrom>()?.EntityName == daughterEntity.Name)?.Name
                ??
                throw new Exception($"Foreign Key from {daughterEntity.Name} was not found!");

    private string GetPrimaryKey(Type daughterEntity) =>
        daughterEntity
            .GetProperties()
            .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.Name
                ??
                throw new Exception($"Primary Key from {daughterEntity.Name} was not found!");

    public void InitSubSelect()
    {
        _config.ScriptSubSelect = "";
        _config.ColumnsSubSelect.Clear();
        _config.WhereListSubSelect.Clear();
        _config.ColumnsSetSubSelect.Clear();
        _config.JoinListSubSelect.Clear();
        _config.OrderByListSubSelect.Clear();
        _config.isCount = false;
    }
    public void ResetSubSelect()
    {
        _config.ColumnsSubSelect.Clear();
        _config.WhereListSubSelect.Clear();
        _config.ColumnsSetSubSelect.Clear();
        _config.JoinListSubSelect.Clear();
        _config.OrderByListSubSelect.Clear();
        _config.isCount = false;
    }

    public string SetColumnsOneToMany(Type nestedListClassType, Type MainTableType, string MainTablePropName, string mainTableAlias, bool isExists = false, JoinEntity join = null, List<string> fullInheritance = null)
    {
        try
        {
            var ScriptSubSelect = new StringBuilder();
            InitSubSelect();
            ScriptSubSelect.Append(GenerateSubSelect(nestedListClassType, isExists, SetSubSelectColumns(nestedListClassType, MainTableType)));
            ScriptSubSelect.Append(GenerateJoinsSubSelect());
            ScriptSubSelect.Append(GenerateWhereSubSelect(nestedListClassType, MainTableType));
            ScriptSubSelect.Append(GenerateOrderBySubSelect());
            ScriptSubSelect.Append(GenerateSubSelectAlias(MainTableType, MainTablePropName, isExists, join, fullInheritance));
            ResetSubSelect();

            return ScriptSubSelect.ToString();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void SetColumnsOneToOne(PropertyInfo nestedClassType, Type mainTableType, string mainPropName, string mainAlias, bool isFromSubSelect = false, JoinAndModel joinAnd = null, List<string> fullInheritance = null, List<string> columnsSubSelect = null)
    {
        var primaryKey = GetPrimaryKey(nestedClassType.PropertyType);
        var foreignKey = GetForeignKey(nestedClassType, mainTableType);
        var nestedTableName = nestedClassType.PropertyType.GetCustomAttribute<TableAttribute>().Name;
        var nestedEntityAlias = UtilsService.GenerateAlias(nestedClassType.Name);

        _config.WhereModelList
         .FindAll(whereModel => whereModel.Entity == nestedClassType.Name && whereModel.MainEntity == mainTableType.Name)
         .ForEach(where => where.EntityAlias = nestedEntityAlias);

        var orderByEntity = _config.OrderByList.Find(orderBy => orderBy.Entity == nestedClassType.Name);
        if (orderByEntity is not null) orderByEntity.EntityAlias = nestedEntityAlias;

        if (isFromSubSelect)
        {
            _config.SubEntitiesWhereList
               .FindAll(subWhere => subWhere.Entity == nestedClassType.Name)
               .ForEach(whereModel => _config.WhereListSubSelect.AddRange(whereModel.WhereList));

            SetJoinsSubSelectScript(nestedTableName, nestedEntityAlias, primaryKey, mainAlias, foreignKey);
        }
        else
            SetJoinsScript(nestedTableName, nestedEntityAlias, primaryKey, mainAlias, foreignKey, joinAnd);

        var primitiveProperties = new List<PropertyInfo>();
        var entitiesProperties = new List<PropertyInfo>();

        void SplitProperties()
        {
            foreach (PropertyInfo mainTableProp in nestedClassType.PropertyType.GetProperties())
            {
                if (mainTableProp.PropertyType.IsClass && mainTableProp.PropertyType != typeof(string))
                {
                    if (mainTableProp.PropertyType.IsGenericType && mainTableProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = mainTableProp.PropertyType.GetGenericArguments()[0];
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == nestedClassType.Name))
                            entitiesProperties.Add(mainTableProp);
                    }
                    else
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == mainTableProp.Name && joinExist.MainEntity == nestedClassType.Name))
                        entitiesProperties.Add(mainTableProp);
                }
                else
                    primitiveProperties.Add(mainTableProp);
            }
        }
        SplitProperties();

        if (_config.isCount is false)
        {
            primitiveProperties.ForEach(primitiveProperty =>
            {
                if (_config.JoinsList.Exists(joinExist => joinExist.Name == nestedClassType.Name && joinExist.JoinFull is true))
                {
                    var join2 = _config.JoinsList.Find(joinExist => joinExist.Entity == nestedClassType.Name && joinExist.JoinFull is true && joinExist.Name == mainPropName);
                    if (isFromSubSelect)
                        columnsSubSelect.Add(SetColumnsDefaultSubSelect(nestedEntityAlias, primitiveProperty.Name, mainPropName, join: join2));
                    else
                    {
                        if (mainTableType.Name == _config.MainEntity.Name)
                            SetColumnsDefault(nestedEntityAlias, primitiveProperty.Name, mainPropName, join: join2, nestedClassType.Name, fullInheritance);
                        else
                            SetColumnsDefault(nestedEntityAlias, primitiveProperty.Name, $"{mainTableType.Name}.{mainPropName}", join: join2, nestedClassType.Name, fullInheritance);
                    }
                }
            });
        }

        foreach (PropertyInfo nestedClassProp in entitiesProperties)
        {
            var EntitiesList = new List<string>(fullInheritance);
            EntitiesList.Add(nestedClassProp.Name);

            var propTypeName = nestedClassProp.PropertyType.Name;

            if (propTypeName != mainTableType.Name)
            {
                if (nestedClassProp.PropertyType.IsGenericType && nestedClassProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type itemType = nestedClassProp.PropertyType.GetGenericArguments()[0];
                    var join = _config.JoinsList.Find(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == nestedClassType.Name);

                    if (join is not null)
                    {
                        var entityColumn = _config.EntitiesColumns.Find(column => column.EntityName == nestedClassType.Name);
                        if (entityColumn is null)
                        {
                            entityColumn = new EntityColumns() { EntityName = _config.MainEntity.Name };
                            entityColumn.Columns.Add(SetColumnsOneToMany(itemType, nestedClassType.PropertyType, nestedClassProp.Name, nestedEntityAlias, false, join: join, fullInheritance));
                            _config.EntitiesColumns.Add(entityColumn);
                        }

                        entityColumn.Columns.Add(SetColumnsOneToMany(itemType, nestedClassType.PropertyType, nestedClassProp.Name, nestedEntityAlias, false, join: join, fullInheritance));
                        continue;
                    }
                }
                else
                {
                    var join = _config.JoinsList.Find(joinFind => joinFind.Entity == nestedClassProp.Name);
                    SetColumnsOneToOne(nestedClassProp, nestedClassType.PropertyType, nestedClassProp.Name, nestedEntityAlias, false, join.JoinAnd, EntitiesList);
                    continue;
                }
            }
        }
    }

    public void SetJoinsScript(string tableName, string tableAlias, string primaryKey, string mainTableAlias, string mainTableForeignKey, JoinAndModel joinAnd = null)
    {
        var joinScript = $"LEFT JOIN {tableName} (NOLOCK) {tableAlias} ON " +
            $"{tableAlias}.{primaryKey} = {mainTableAlias}.{mainTableForeignKey}";

        if (joinAnd?.Entity is not null)
            joinScript += $" AND {tableAlias}.{joinAnd.EntityProp} {joinAnd.SqlOperator} {joinAnd.Value}";

        _config.JoinList.Add(joinScript);
    }

    public void SetJoinsSubSelectScript(string tableName, string tableAlias, string primaryKey, string mainTableAlias, string mainTableForeignKey) =>
        _config.JoinListSubSelect.Add($"LEFT JOIN {tableName} (NOLOCK) {tableAlias} ON {tableAlias}.{primaryKey} = {mainTableAlias}.{mainTableForeignKey}");

    private string GetForeignKeyFromSubSelect(Type nestedListClassType, Type MainTableType) =>
        nestedListClassType
            .GetProperties()
            .FirstOrDefault(prop => prop.GetCustomAttribute<ForeignkeyFrom>()?.EntityName == MainTableType.Name)?.Name ??
        throw new Exception($"Foreign Key from {MainTableType.Name} was not found!");

    protected virtual List<string> SetSubSelectColumns(Type nestedListClassType, Type MainTableType)
    {
        var columnsSubSelect = new List<string>();
        var foreignKey = GetForeignKeyFromSubSelect(nestedListClassType, MainTableType);
        var nestedTable = nestedListClassType.GetCustomAttribute<TableAttribute>().Name;
        var NestedAlias = UtilsService.GenerateAlias(nestedListClassType.Name);
        var orderByEntity = _config.OrderByList.Find(orderBy => orderBy.Entity == nestedListClassType.Name);

        if (orderByEntity is not null)
        {
            orderByEntity.EntityAlias = _config.NestedAlias;
            _config.OrderByListSubSelect.Add(orderByEntity);
            _config.OrderByList.Remove(orderByEntity);
        }

        if (_config.CountList.Exists(count => count.Entity == nestedListClassType.Name))
        {
            var count = _config.CountList.Find(count => count.Entity == nestedListClassType.Name);

            //_config.ColumnsSubSelect.Add($" COUNT(*) ");
            columnsSubSelect.Add($" COUNT(*) ");

            _config.isCount = true;
        }

        var primitiveProperties = new List<PropertyInfo>();
        var entitiesProperties = new List<PropertyInfo>();

        void SplitProperties()
        {
            foreach (PropertyInfo mainTableProp in nestedListClassType.GetProperties())
            {
                if (mainTableProp.PropertyType.IsClass && mainTableProp.PropertyType != typeof(string))
                {
                    if (mainTableProp.PropertyType.IsGenericType && mainTableProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = mainTableProp.PropertyType.GetGenericArguments()[0];
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == nestedListClassType.Name))
                            entitiesProperties.Add(mainTableProp);
                    }
                    else
                        if (_config.JoinsList.Exists(joinExist => joinExist.Entity == mainTableProp.Name && joinExist.MainEntity == nestedListClassType.Name))
                        entitiesProperties.Add(mainTableProp);
                }
                else
                    primitiveProperties.Add(mainTableProp);
            }
        }
        SplitProperties();

        if (_config.isCount is false) primitiveProperties.ForEach(primitiveProperty => columnsSubSelect.Add(SetColumnsDefaultNoAliasSubSelect(NestedAlias, primitiveProperty.Name)));

        foreach (PropertyInfo nestedListTableProp in entitiesProperties)
        {
            if (nestedListTableProp.PropertyType.Name != MainTableType.Name)
            {
                if (nestedListTableProp.PropertyType.IsGenericType && nestedListTableProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type itemType = nestedListTableProp.PropertyType.GetGenericArguments()[0];

                    var join = _config.JoinsList.Find(joinExist => joinExist.Entity == itemType.Name && joinExist.MainEntity == nestedListClassType.Name);
                    columnsSubSelect.Add(SetColumnsOneToMany(itemType, nestedListClassType, nestedListTableProp.Name, NestedAlias, join: join));
                    continue;
                }
                else
                {
                    SetColumnsOneToOne(nestedListTableProp, nestedListClassType, nestedListTableProp.Name, NestedAlias, true, columnsSubSelect: columnsSubSelect);
                    continue;
                }
            }
            else
                continue;
        }

        return columnsSubSelect;
    }

    public void SetWhereSubSelect(Type nestedListClassType, Type MainTableType)
    {
        _config.WhereListSubSelect.Add($"{UtilsService.GenerateAlias(MainTableType.Name)}.{GetPrimaryKey(MainTableType)} = " +
            $"{UtilsService.GenerateAlias(nestedListClassType.Name)}" +
            $".{GetForeignKeyFromSubSelect(nestedListClassType, MainTableType)}");

        var whereEntitys = _config.WhereModelList
            .FindAll(whereModel => whereModel.Entity == nestedListClassType.Name || whereModel.MainEntity == nestedListClassType.Name);

        whereEntitys.ForEach(whereModel =>
        _config.WhereListSubSelect.Add($"{whereModel.EntityAlias ?? _config.NestedAlias}.{whereModel.EntityProp} {whereModel.SqlOperator} {whereModel.Value}"));

        _config.WhereModelList = _config.WhereModelList.Except(whereEntitys).ToList();
    }

    protected void SetColumnsDefault(string aliasTabela, string columnsName, string subEntityName = "", JoinEntity join = null, string EntityName = "", List<string> fullInheritance = null)
    {
        var entityColumns = _config.EntitiesColumns.Find(columns => columns.EntityName == EntityName);

        if (entityColumns is null)
        {
            entityColumns = new EntityColumns() { EntityName = EntityName };

            if (subEntityName.Length > 0)
                entityColumns.Columns.Add($"{aliasTabela}.{columnsName} AS [{string.Join(".", fullInheritance)}.{columnsName}]");
            else
                entityColumns.Columns.Add($"{aliasTabela}.{columnsName} AS '{columnsName}{subEntityName}'");

            _config.EntitiesColumns.Add(entityColumns);
        }
        else
        {
            if (subEntityName.Length > 0)
                entityColumns.Columns.Add($"{aliasTabela}.{columnsName} AS [{string.Join(".", fullInheritance)}.{columnsName}]");

            else
                entityColumns.Columns.Add($"{aliasTabela}.{columnsName} AS '{columnsName}{subEntityName}'");
        }
    }

    protected string SetColumnsDefaultSubSelect(string aliasTabela, string columnsName, string subEntityName = "", JoinEntity join = null)
    {
        if (subEntityName.Length > 0)
            return $"{aliasTabela}.{columnsName} AS [{subEntityName}.{columnsName}]";
        else
            return $"{aliasTabela}.{columnsName} AS '{columnsName}{subEntityName}'";
    }

    protected void SetMainEntityColumnsDefaultNoAlias(string columnsName)
    {
        var caseModel = _config.Cases.Find(caseModel => caseModel.Entity == _config.MainEntity.Name && caseModel.EntityProp == columnsName);

        if (caseModel is not null) SetColumnsCase(caseModel);

        else
        {
            var entityColumn = _config.EntitiesColumns.Find(column => column.EntityName == _config.MainEntity.Name);
            if (entityColumn is null)
            {
                entityColumn = new EntityColumns() { EntityName = _config.MainEntity.Name };
                entityColumn.Columns.Add($"{_config.MainAlias}.{columnsName}");
                _config.EntitiesColumns.Add(entityColumn);
            }
            else
                entityColumn.Columns.Add($"{_config.MainAlias}.{columnsName}");
        }
    }

    protected string SetColumnsDefaultNoAliasSubSelect(string alias, string columnsName) => $"{alias}.{columnsName}";

    protected void GenerateWhere()
    {
        _config.WhereList.AddRange(_config.WhereModelList.Select(where =>
        {
            if (where is DateDiffDay whereDataDiffDayFunc)
            {
                return $"{whereDataDiffDayFunc.FunctionName}({string.Join($",", whereDataDiffDayFunc.Args)} " +
                        $",{where.EntityAlias}.{where.EntityProp}) {where.SqlOperator} {where.Value}";
            }
            else
            {
                return $"{where.EntityAlias}.{where.EntityProp} {where.SqlOperator} {where.Value}";
            }
        }));

        _config.MainEntityWhereList.ForEach(w => _config.WhereList.AddRange(w.WhereList));

        if (_config.WhereList.Count > 0) _config.Script += $"{CodesEnum.BR}WHERE {string.Join($" {CodesEnum.BR} AND ", _config.WhereList)}";
    }

    protected string GenerateWhereSubSelect(Type nestedListClassType, Type MainTableType)
    {
        SetWhereSubSelect(nestedListClassType, MainTableType);

        _config.SubEntitiesWhereList
            .FindAll(subWhere => subWhere.Entity == nestedListClassType.Name)
            .ForEach(whereModel => _config.WhereListSubSelect.AddRange(whereModel.WhereList));

        if (_config.WhereListSubSelect.Count == 0) return "";

        return $"{CodesEnum.BR}WHERE {string.Join($" {CodesEnum.BR} AND ", _config.WhereListSubSelect)}";
    }

    protected string GenerateSubSelectAlias(Type MainTableType, string MainTablePropName, bool isExists, JoinEntity join = null, List<string> fullInheritance = null)
    {
        var subSelectAlias = new StringBuilder();
        if (isExists)
            subSelectAlias.Append($")");

        else if (MainTableType.Name == _config.MainEntity.Name)
            subSelectAlias.Append(_config.isCount ? $") AS  {_config.MainPropName}" : $" FOR JSON PATH) AS  {_config.MainPropName}");

        else
        {
            if (_config.isCount)
            {
                subSelectAlias.Append($") AS ");
                subSelectAlias.Append(_config.ScriptSubSelect += _config.MainPropName == MainTableType.Name ?
                    $"[{_config.MainPropName}.{MainTablePropName}]"
                    : $"[{_config.MainPropName}.{MainTableType.Name}.{MainTablePropName}]");
            }
            else
            {
                if (_config.MainEntity.Name == _config.MainPropName)
                {
                    subSelectAlias.Append($" FOR JSON PATH) ");
                    subSelectAlias.Append(_config.MainPropName == MainTableType.Name ?
                        $"[{_config.MainPropName}.{MainTablePropName}]"
                        : $"[{_config.MainPropName}.{MainTableType.Name}.{MainTablePropName}]");
                }
                else
                {
                    if (fullInheritance is null)
                    {

                        subSelectAlias.Append($" FOR JSON PATH) ");
                        subSelectAlias.Append($"[{MainTablePropName}]");
                    }
                    else
                    {
                        subSelectAlias.Append($" FOR JSON PATH) ");
                        subSelectAlias.Append($"[" +
                            $"{string.Join(".", fullInheritance)}.{MainTablePropName}]");
                    }
                }
            }

        }

        return subSelectAlias.ToString();
    }

    protected virtual void GeneratePaginacao()
    {
        if (_config.Pagination.HasPagination is true)
        {
            _config.Script += $"{CodesEnum.BR}OFFSET({_config.Pagination.Page} - 1) * {_config.Pagination.PageSize} ROWS" +
            $"{CodesEnum.BR}FETCH NEXT {_config.Pagination.PageSize} ROWS ONLY";
        }
    }

    protected void GenerateJoins() => _config.JoinList.ForEach(join => _config.Script += $"{CodesEnum.BR}{string.Join(CodesEnum.BR, join)} ");

    protected string GenerateJoinsSubSelect()
    {
        if (_config.JoinListSubSelect.Count > 0)
            return string.Join(CodesEnum.BR, _config.JoinListSubSelect.ConvertAll(join => $"{CodesEnum.BR}{string.Join(CodesEnum.BR, join)} "));
        else
            return "";
    }

    protected virtual void GenerateMainSelect()
    {
        string select = $"{CodesEnum.BRDOUBLE}";

        select += $"{CodesEnum.BR}SELECT";

        if (_config.IsFirst) select += $@" TOP(1) ";

        _config.EntitiesColumns.ForEach(entity => _config.Columns.AddRange(entity.Columns));

        select += $"{CodesEnum.BRTAB}{string.Join($"{CodesEnum.BRTAB},", [.. _config.Columns])} ";

        select += $"{CodesEnum.BR}FROM {_config.MainTable} (NOLOCK) {_config.MainAlias} ";

        _config.Script += select;
    }

    protected virtual string GenerateSubSelect(Type nestedListClassType, bool isExists, List<string> columnsSubSelect)
    {
        string select = $"{CodesEnum.BRDOUBLE}";

        if (isExists) select += "EXISTS ";

        select += $"{CodesEnum.BR}(SELECT";

        if (_config.TopModelOfList.Exists(top => top.Entity == nestedListClassType.Name)) select += $@" TOP(1) ";

        select += $"{CodesEnum.BRTAB}{string.Join($"{CodesEnum.BRTAB},", [.. columnsSubSelect])} ";

        select += $"{CodesEnum.BR}FROM {nestedListClassType.GetCustomAttribute<TableAttribute>().Name} (NOLOCK) {UtilsService.GenerateAlias(nestedListClassType.Name)} ";

        return select;
    }

    protected virtual void GenerateOrderBy()
    {
        if (_config.OrderByList.Count > 0)
            _config.Script += $"{CodesEnum.BR}ORDER BY {string.Join(", ", _config.OrderByList.Select(orderBy => $"{orderBy.EntityAlias}.{orderBy.EntityProp} {orderBy.Direction}"))}";
    }

    protected virtual string GenerateOrderBySubSelect()
    {
        if (_config.OrderByListSubSelect.Count > 0)
            return $"{CodesEnum.BR}ORDER BY {string.Join(", ", _config.OrderByListSubSelect.Select(orderBy => $"{orderBy.EntityAlias}.{orderBy.EntityProp} {orderBy.Direction}"))}";
        else
            return "";
    }

    protected void SetColumnsCase(CaseColumnModel caseModel)
    {
        caseModel.ColumnsDefault.ForEach(itemColumn =>
        {
            ColumnSet columnCaseFind = caseModel.ColumnsReplace.Find(columnFind => columnFind.Name == itemColumn);

            if (columnCaseFind == null)
                SetColumnsDefault(caseModel.Alias, itemColumn, caseModel.ObjectName);

            else
            {
                string aliasColumn = itemColumn;
                if (UtilsService.IsNotNull(caseModel.ObjectName))
                {
                    aliasColumn = $"[{caseModel.ObjectName}.{itemColumn}]";
                }

                _config.Columns.Add(
                    $@"CASE
                    WHEN {caseModel.Condition}
                        THEN {caseModel.Alias}.{itemColumn}  
                    ELSE
                        {columnCaseFind.GetValue()}
                    END  AS {aliasColumn}");
            }
        });
    }

    private void SetColumnsCase(CaseModel caseModel)
    {
        StringBuilder Case = new StringBuilder();
        Case.Append($@"CASE");

        caseModel.Whens.ForEach(when =>
        {
            Case.Append($@" WHEN {when.EntityAlias}.{when.EntityProp} {when.Operator} {when.WhenValue}
                                THEN {when.ThenValue}
            ");
        });

        Case.Append($@" ELSE {caseModel.Else}");
        Case.Append($@" END  AS {caseModel.End}");

        _config.Columns.Add(Case.ToString());
    }
}