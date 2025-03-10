using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using stORM.utils;
using static stORM.Models.GroupByModel;
using static stORM.Models.GroupByModel.Config;

namespace stORM.Models;


public class CaseColumnModel
{
    public List<string> ColumnsDefault { get; set; } = [];
    public List<ColumnSet> ColumnsReplace { get; set; } = [];
    public string Condition { get; set; } = "";
    public string Alias { get; set; } = "";
    public string ObjectName { get; set; } = "";

}

public class JoinEntity
{
    public string MainEntity { get; set; } = "";
    public string Entity { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullInheritance { get; set; } = "";
    public bool JoinFull { get; set; } = false;
    public JoinAndModel JoinAnd { get; set; } = new JoinAndModel();
}

public class JoinAndModel
{
    public string Entity { get; set; }
    public string EntityProp { get; set; }
    public string SqlOperator { get; set; }
    public string Value { get; set; }
}

public class JoinModel
{
    public string Table { get; set; } = "";
    public string Alias { get; set; } = "";
    public string LeftAlias { get; set; } = "";
    public string ForeignKey { get; set; } = "";
    public string PrimaryKey { get; set; } = "id";
    public string ParticularScript { get; set; } = "";
    public string AdditionalScript { get; set; } = "";

    public string Type { get; set; } = "INNER";
    public string Script { get; set; } = "";
    public string SubqueryScript { get; set; } = "";
    public string SubSelectScript { get; set; } = "";

    public List<string> Origens { get; set; } = [];

    public void SetScript()
    {
        if (Script is null)
        {
            if (ParticularScript is not null)
            {
                Script = $"{Type} JOIN {Table} (NOLOCK) {Alias} ON {ParticularScript}";
            }
            else
            {
                Script = $"{Type} JOIN {Table} (NOLOCK) {Alias} ON {Alias}.{PrimaryKey} = {LeftAlias}.{ForeignKey}";
            }

            Script += AdditionalScript;
        }
    }
}

public class GroupByModel
{
    public string Key = "id";
    public List<string> AliasJoin = [];

    public GroupByModel()
    {
    }

    public GroupByModel(string key, List<string> aliasJoin)
    {
        Key = key;
        AliasJoin = aliasJoin;
    }

    public class ColumnAlias
    {
        public string Column { get; set; } = "";
        public string Alias { get; set; } = "";
    }

    public class ColumnSet
    {
        public string Name { get; set; } = "";
        private string? ValueString { get; set; } = null;
        private long? ValueInt { get; set; } = null;
        private double? ValueDouble { get; set; } = null;
        private bool? ValueBool { get; set; } = null;
        private DateTime ValueDate { get; set; } = DateTime.MinValue;

        public ColumnSet(string name, dynamic value)
        {
            Name = name;
            this.SetValue(value);
        }

        public void SetValue(string value)
        {
            ValueString = $"{value}";
        }
        public void SetValue(long value)
        {
            ValueInt = value;
        }
        public void SetValue(double value)
        {
            ValueDouble = value;
        }
        public void SetValue(bool value)
        {
            ValueBool = value;
        }
        public void SetValue(DateTime value)
        {
            ValueDate = value;
        }

        public string GetValue()
        {
            if (ValueString != null)
            {
                return $"'{ValueString.Replace("'", "''")}'";
            }
            if (ValueInt != null)
            {
                return $"{ValueInt}";
            }
            if (ValueDouble != null)
            {
                return $"{ValueDouble}";
            }
            if (ValueBool != null)
            {
                return $"{(ValueBool == true ? '1' : '0')}";
            }
            if (ValueDate != DateTime.MinValue)
            {
                return $"'{ValueDate:yyyyMMdd HH:mm:ss.fff}'";
            }

            return "";
        }

    }

    public static class CodesEnum
    {
        public const string BR = "\r\n";
        public const string BRTAB = "\r\n  ";
        public const string BRTAB2 = "\r\n   ";
        public const string BRTAB3 = "\r\n       ";
        public const string BRTAB4 = "\r\n         ";
        public const string BRTAB5 = "\r\n            ";
        public const string BRTAB6 = "\r\n                ";
        public const string BRDOUBLE = "\r\n\r\n";
        public const string TAB = " ";
        public const string TABDOUBLE = "  ";
        public const string TABTRIPLE = "    ";
    }

    public class WhereModel
    {
        public string MainEntity { get; set; }
        public string Entity { get; set; }
        public string EntityAlias { get; set; }
        public string EntityProp { get; set; }
        public string SqlOperator { get; set; }
        public string Value { get; set; }
        public string WhereExpression { get; set; }
    }

    public class WhereFuncModel : WhereModel
    {
        public string FunctionName { get; set; } = "";
        public List<string> Args { get; set; } = new List<string>();
    }

    public class DateDiffDay : WhereFuncModel
    {

    }
    public class ExistsModel : WhereFuncModel
    {

    }

    public class OrderByModel
    {
        public string Entity { get; set; } = "";
        public string EntityAlias { get; set; } = "";
        public string EntityProp { get; set; } = "";
        public string Direction { get; set; } = "ASC";
    }

    public class Pagination
    {
        public bool HasPagination { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CaseModel
    {
        public string Entity { get; set; } = "";
        public string EntityAlias { get; set; } = "";
        public string EntityProp { get; set; } = "";
        public List<WhenModel> Whens { get; set; } = new List<WhenModel>();
        public string Else { get; set; } = "";
        public string End { get; set; } = "";

    }

    public class WhenModel
    {
        public string Entity { get; set; }
        public string EntityAlias { get; set; } = "";
        public string EntityProp { get; set; } = "";
        public string WhenValue { get; set; } = "";
        public string ThenValue { get; set; } = "";
        public string Operator { get; set; } = "";
    }

    public class CountModel
    {
        public string Entity { get; set; } = "";
        public string Alias { get; set; } = "";
    }

    public class TopModelOf
    {
        public string Entity { get; set; } = "";
        public string number { get; set; } = "";
    }

    public class WhereModelQuery
    {
        public string Entity { get; set; } = "";
        public List<string> WhereList { get; set; } = new List<string>();
    }

    public class Config
    {
        State _state = new State();

        public void SetEntity(Type entity)
        {
            MainEntity = entity;
        }

        public void Initialize()
        {
            Script = "";
            Columns = [];
            Joins = [];
            ColumnsSet = [];
            JoinList.Clear();
        }

        public Config DeriveNew()
        {
            _state = new State();
            return this;
        }

        public class State
        {
            public string Script { get; set; } = "";
            public Type MainEntity { get; set; }

            public List<string> Columns { get; set; } = new List<string>();
            public List<string> SubselectColumns { get; set; } = new List<string>();
            public List<ColumnSet> ColumnsSet { get; set; } = new List<ColumnSet>();

            public List<EntityColumns> EntitiesColumns { get; set; } = new List<EntityColumns>();

            public string MainTable { get; set; } = "";
            public string MainAlias { get; set; } = "";

            public List<WhereModelQuery> MainEntityWhereList { get; set; } = new List<WhereModelQuery>();
            public List<WhereModelQuery> SubEntitiesWhereList { get; set; } = new List<WhereModelQuery>();

            public List<string> WhereList { get; set; } = new List<string>();
            public List<WhereModel> WhereModelList { get; set; } = new List<WhereModel>();
            public List<JoinModel> Joins { get; set; } = new List<JoinModel>();
            public List<string> JoinList { get; set; } = new List<string>();
            public List<string> JoinListSubSelect { get; set; } = new List<string>();
            public string ScriptSubSelect { get; set; } = "";
            public string ScriptExistSubSelect { get; set; } = "";
            public List<string> ColumnsSubSelect { get; set; } = new List<string>();
            public List<string> WhereListSubSelect { get; set; } = new List<string>();
            public List<string> ColumnsSetSubSelect { get; set; } = new List<string>();
            public string NestedTable { get; set; } = "";
            public string NestedAlias { get; set; } = "";
            public string NestedForeignKey { get; set; } = "";
            public string MainPropName { get; set; } = "";
            public List<JoinEntity> JoinsList { get; set; } = new List<JoinEntity>();
            public List<ExistsModel> ExistsList { get; set; } = new List<ExistsModel>();
            public List<OrderByModel> OrderByList { get; set; } = new List<OrderByModel>();
            public List<OrderByModel> OrderByListSubSelect { get; set; } = new List<OrderByModel>();
            public CaseModel Case { get; set; } = new CaseModel();
            public List<CaseModel> Cases { get; set; } = new List<CaseModel>();
            public Pagination Pagination { get; set; } = new Pagination();
            public bool IsFirst { get; set; } = false;
            public List<CountModel> CountList { get; set; } = new List<CountModel>();
            public bool isCount { get; set; } = false;
            public List<TopModelOf> TopModelOfList { get; set; } = new List<TopModelOf>();
        }

        public Type MainEntity
        {
            get => _state.MainEntity;
            set => _state.MainEntity = value;
        }

        public string Script
        {
            get => _state.Script;
            set => _state.Script = value;
        }

        public List<string> Columns
        {
            get => _state.Columns;
            set => _state.Columns = value;
        }
        public List<string> SubselectColumns
        {
            get => _state.SubselectColumns;
            set => _state.SubselectColumns = value;
        }

        public List<ColumnSet> ColumnsSet
        {
            get => _state.ColumnsSet;
            set => _state.ColumnsSet = value;
        }
        public List<EntityColumns> EntitiesColumns
        {
            get => _state.EntitiesColumns;
            set => _state.EntitiesColumns = value;
        }

        public string MainTable
        {
            get => _state.MainTable;
            private set => _state.MainTable = value;
        }

        public string MainAlias
        {
            get => _state.MainAlias;
            private set => _state.MainAlias = value;
        }

        public List<WhereModelQuery> MainEntityWhereList
        {
            get => _state.MainEntityWhereList;
            set => _state.MainEntityWhereList = value;
        }
        public List<WhereModelQuery> SubEntitiesWhereList
        {
            get => _state.SubEntitiesWhereList;
            set => _state.SubEntitiesWhereList = value;
        }

        public List<string> WhereList
        {
            get => _state.WhereList;
            set => _state.WhereList = value;
        }

        public List<WhereModel> WhereModelList
        {
            get => _state.WhereModelList;
            set => _state.WhereModelList = value;
        }

        public List<JoinModel> Joins
        {
            get => _state.Joins;
            set => _state.Joins = value;
        }

        public List<string> JoinList
        {
            get => _state.JoinList;
            set => _state.JoinList = value;
        }

        public List<string> JoinListSubSelect
        {
            get => _state.JoinListSubSelect;
            set => _state.JoinListSubSelect = value;
        }

        public string ScriptSubSelect
        {
            get => _state.ScriptSubSelect;
            set => _state.ScriptSubSelect = value;
        }
        public string ScriptExistSubSelect
        {
            get => _state.ScriptExistSubSelect;
            set => _state.ScriptExistSubSelect = value;
        }

        public List<string> ColumnsSubSelect
        {
            get => _state.ColumnsSubSelect;
            set => _state.ColumnsSubSelect = value;
        }

        public List<string> WhereListSubSelect
        {
            get => _state.WhereListSubSelect;
            set => _state.WhereListSubSelect = value;
        }

        public List<string> ColumnsSetSubSelect
        {
            get => _state.ColumnsSetSubSelect;
            set => _state.ColumnsSetSubSelect = value;
        }

        public string NestedTable
        {
            get => _state.NestedTable;
            set => _state.NestedTable = value;
        }

        public string NestedAlias
        {
            get => _state.NestedAlias;
            set => _state.NestedAlias = value;
        }

        public string NestedForeignKey
        {
            get => _state.NestedForeignKey;
            set => _state.NestedForeignKey = value;
        }

        public string MainPropName
        {
            get => _state.MainPropName;
            set => _state.MainPropName = value;
        }

        public List<JoinEntity> JoinsList
        {
            get => _state.JoinsList;
            set => _state.JoinsList = value;
        }

        public List<ExistsModel> ExistsList
        {
            get => _state.ExistsList;
            set => _state.ExistsList = value;
        }

        public List<OrderByModel> OrderByList
        {
            get => _state.OrderByList;
            set => _state.OrderByList = value;
        }

        public List<OrderByModel> OrderByListSubSelect
        {
            get => _state.OrderByListSubSelect;
            set => _state.OrderByListSubSelect = value;
        }

        public CaseModel Case
        {
            get => _state.Case;
            set => _state.Case = value;
        }

        public List<CaseModel> Cases
        {
            get => _state.Cases;
            set => _state.Cases = value;
        }

        public Pagination Pagination
        {
            get => _state.Pagination;
            set => _state.Pagination = value;
        }

        public bool IsFirst
        {
            get => _state.IsFirst;
            set => _state.IsFirst = value;
        }

        public List<CountModel> CountList
        {
            get => _state.CountList;
            set => _state.CountList = value;
        }

        public bool isCount
        {
            get => _state.isCount;
            set => _state.isCount = value;
        }
        public List<TopModelOf> TopModelOfList
        {
            get => _state.TopModelOfList;
            set => _state.TopModelOfList = value;
        }

        public void ConfigMainEntity()
        {
            if (MainEntity.Name is not null)
            {
                MainTable = MainEntity.GetCustomAttribute<TableAttribute>().Name;
                MainAlias = UtilsService.GenerateAlias(MainEntity.Name);

                ConfigMainEntityInWhereModelList();
                ConfigMainEntityInOrderByList();
                ConfigMainEntityInCountList();
            }
            else
            {
                throw new Exception("Main Entity can not be null");
            }
        }

        public void ConfigMainEntityInWhereModelList() =>
            WhereModelList
                .Where(whereModel => whereModel.Entity == MainEntity.Name)
                .ToList()
                .ForEach(where => where.EntityAlias = MainAlias);

        public void ConfigMainEntityInOrderByList()
        {
            var orderByEntity = OrderByList.Find(orderBy => orderBy.Entity == MainEntity.Name);
            if (orderByEntity is not null) orderByEntity.EntityAlias = MainAlias;
        }

        public void ConfigMainEntityInCountList()
        {
            var count = CountList.Find(count => count.Entity == MainEntity.Name);
            if (count is not null)
            {
                Columns.Add($" COUNT(*) AS 'COUNT' ");
                isCount = true;
            }
        }
    }
    public class AliasModel
    {
        public string Name { get; set; }
        public int index { get; set; }
    }
    public class EntityColumns
    {
        public string EntityName { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
    }
}

public static class BaseScripts
{
    public static string JSON_START = $"{CodesEnum.BR}DECLARE @JSON nvarchar(max) {CodesEnum.BR}SET @JSON = (";

    public static string JSON_END = $"{CodesEnum.BR} FOR JSON PATH) SELECT @JSON AS 'result'";
}
