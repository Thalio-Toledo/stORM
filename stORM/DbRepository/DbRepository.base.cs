using System.Linq.Expressions;
using BonesCore.BonesCoreOrm;
using static stORM.Models.GroupByModel;
using BonesCore.BonesCoreOrm.ExpressionsTranslators;
using confirp_bonescore.BonesCoreOrm.ExpressionsTranslators;
using BonesCoreOrm.Generators;
using stORM.Models;
using stORM.stORM_Core;

namespace stORM.DbRepository;

public class DbRepository<T>
{
    public stORMCore stORMCore { get; set; }
    public DbRepository(stORMCore orm)
    {
        stORMCore = orm;
    }

    private void setConfigInORM()
    {
        Config config;

        if (stORMCore._config is null)
        {
            config = new Config();
            config.SetEntity(typeof(T));
            stORMCore.SetConfig(config);
        }
        else
        {
            if (stORMCore._config.MainEntity != typeof(T))
            {
                config = new Config();
                config.SetEntity(typeof(T));
                stORMCore.SetConfig(config);
            }
        }
    }

    /// <summary>
    /// Do not use this contructor. This method is only used to create a mock object of the repository for unit tests.
    /// </summary>
    public DbRepository() { }

    public List<T> Query(string sqlScript, object parameters = null) =>
        Mapper.Map<T>(stORMCore.Query(sqlScript, parameters));

    public T QueryFirst<T>(string sqlScript, object parameters = null) =>
        Mapper.Map<T>(stORMCore.QueryFirst(sqlScript, parameters)).First();


    /// <summary>
    /// This method is used to get the first result of query. Its like a SELECT TOP(1)
    /// </summary>
    /// <returns></returns>
    public virtual dynamic FirstOrDefault()
    {
        setConfigInORM();
        stORMCore._config.IsFirst = true;
        return Mapper.Map<T>(stORMCore.Generate<SelectGen>()).First();
    }

    /// <summary>
    /// This method is used to get the first result of a different entity on query. It is like SELECT TOP(1), and is most used
    /// when you need to make a case query.
    /// </summary>
    /// <typeparam name="Generic"></typeparam>
    /// <returns>Returns the one element of the Generic type passed as parameter.</returns>
    public virtual dynamic FirstOrDefault<Generic>()
    {
        setConfigInORM();
        return Mapper.Map<Generic>(stORMCore.Generate<SelectGen>()).First();
    }

    public virtual DbRepository<T> FirstOrDefaultOf<TProperty>(Expression<Func<T, TProperty>> predicate)
    {
        setConfigInORM();

        var translator = new FirstOrDefaultOfTranslator(typeof(T), predicate.Body);
        var top = translator.TranslateExpression();
        top.number = "1";

        stORMCore._config.TopModelOfList.Add(top);

        return this;
    }

    public virtual List<T> ToList()
    {
        setConfigInORM();
        return Mapper.Map<T>(stORMCore.Generate<SelectGen>());
    }

    public async virtual Task<List<T>> ToListAsync()
    {
        setConfigInORM();
        return Mapper.Map<T>(await stORMCore.GenerateAsync<SelectGen>());
    }

    public virtual List<E> ToList<E>()
    {
        setConfigInORM();
        return Mapper.Map<E>(stORMCore.Generate<SelectGen>());
    }

    public virtual int Count()
    {
        setConfigInORM();
        stORMCore._config.CountList.Add(new CountModel { Entity = typeof(T).Name });
        return Mapper.Count(stORMCore.Generate<SelectGen>());
    }

    public virtual DbRepository<T> CountOf<TProperty>(Expression<Func<T, TProperty>> predicate)
    {
        setConfigInORM();
        stORMCore._config.CountList.Add(new CountOfTranslator(typeof(T), predicate.Body).TranslateExpression());

        return this;
    }

    public virtual DbRepository<T> Where(Expression<Func<T, bool>> predicate)
    {
        setConfigInORM();
        var translator = new WhereTranslator(typeof(T));
        var whereInfo = translator.TranslateWhereExpression(predicate.Body);

        var whereModelQuery = new WhereModelQuery();
        whereModelQuery.Entity = typeof(T).Name;

        var entities = whereInfo.Entities;
        whereModelQuery.WhereList.Add(whereInfo.WhereScript);

        stORMCore._config.MainEntityWhereList.Add(whereModelQuery);

        foreach (var entity in entities)
        {
            if (entity != typeof(T).Name)
            {
                var existInJoinList = stORMCore._config.JoinsList.Exists(join => join.Entity == entity);
                if (existInJoinList is false)
                {
                    var join = new JoinEntity
                    {
                        Entity = entity
                    };
                    stORMCore._config.JoinsList.Add(join);
                }
            }
        }

        return this;
    }

    public virtual DbRepository<T> Where<TProperty>(Expression<Func<T, TProperty>> predicate)
    {
        setConfigInORM();

        var translator = new WhereOfTranslator(typeof(T), predicate.Body);
        var where = translator.TranslateExpression();

        stORMCore._config.WhereModelList.Add(where);

        return this;
    }

    public virtual DbRepository<T> Exists<TInner>(Expression<Func<T, IEnumerable<TInner>>> innerSelector)
    {
        setConfigInORM();

        var methodName = ExtractMethod(innerSelector);

        var te = ExtractInnerSelector(innerSelector);
        var tes = ExtractWherePredicate(innerSelector);

        if (methodName == "Where")
        {
            var translator = new ExistsTranslator(typeof(T), te);
            var where = translator.TranslateExpression();

            var existInWhereList = stORMCore._config.ExistsList
                .Exists(whereExists => whereExists is WhereFuncModel wherefunc && wherefunc.FunctionName == where.FunctionName);

            if (existInWhereList is false)
            {
                stORMCore._config.ExistsList.Add(where);
            }
            else
            {
                stORMCore._config.ExistsList = stORMCore._config.ExistsList.FindAll(whereExists => whereExists.Entity != whereExists.Entity);
                stORMCore._config.ExistsList.Add(where);
            }

            var whereTranslator = new WhereTranslator(typeof(T), tes);
            var innerWhere = whereTranslator.TranslateExpression();

            stORMCore._config.WhereModelList.Add(innerWhere);
        }

        return this;
    }

    public virtual DbRepository<T> DateDiffDay(Expression<Func<int, bool>> predicate = default)
    {
        setConfigInORM();

        var translator = new DateDiffTranslator(typeof(T), predicate.Body);
        var whereDateDiffDay = translator.TranslateExpression();

        whereDateDiffDay.Args.Add("DAY");
        whereDateDiffDay.Args.Add("GETDATE()");

        var where = stORMCore._config.WhereModelList.Last();
        whereDateDiffDay.Entity = where.Entity;
        whereDateDiffDay.EntityProp = where.EntityProp;
        stORMCore._config.WhereModelList.Remove(where);
        stORMCore._config.WhereModelList.Add(whereDateDiffDay);

        return this;
    }

    public virtual DbRepository<T> IN(int[] values)
    {
        setConfigInORM();
        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "IN(";
        where.Value = $"{string.Join(",", values)})";

        return this;
    }

    public virtual DbRepository<T> IN(string[] values)
    {
        setConfigInORM();
        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "IN(";
        where.Value = $"{string.Join(",", $" '{values}' ")} )";
        return this;

    }

    public virtual DbRepository<T> IN(List<int> values)
    {
        setConfigInORM();
        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "IN(";
        where.Value = $"{string.Join(",", values)})";

        return this;
    }

    public virtual DbRepository<T> IN(List<string> values)
    {
        setConfigInORM();

        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "IN(";
        where.Value = $"{string.Join(",", $" '{values}' ")} )";
        return this;
    }

    public virtual DbRepository<T> NotIN(int[] values)
    {
        setConfigInORM();

        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "NOT IN(";
        where.Value = $"{string.Join(",", values)})";

        return this;
    }

    public virtual DbRepository<T> NotIN(string[] values)
    {
        setConfigInORM();

        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = " NOT IN(";
        where.Value = $"{string.Join(",", $" '{values}' ")} )";
        return this;
    }

    public virtual DbRepository<T> NotIN(List<int> values)
    {
        setConfigInORM();
        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = "NOT IN(";
        where.Value = $"{string.Join(",", values)})";

        return this;
    }
    public virtual DbRepository<T> NotIN(List<string> values)
    {
        setConfigInORM();

        var where = stORMCore._config.WhereModelList.Last();

        where.SqlOperator = " NOT IN(";
        where.Value = $"{string.Join(",", $" '{values}' ")} )";
        return this;
    }

    public virtual DbRepository<T> Join<TProperty>(Expression<Func<T, TProperty>> predicate)
    {
        setConfigInORM();

        static List<Expression> SplitThenJoin<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var Expressions = new List<Expression>();

            void ProcessExpression(Expression exp)
            {
                if (exp is MethodCallExpression methodCall && (methodCall.Method.Name == "ThenJoin" || methodCall.Method.Name == "Where"))
                {
                    var ExpressionsAux = new List<Expression>();

                    ExpressionsAux.Add(methodCall.Arguments[0]);
                    ExpressionsAux.Add(methodCall.Arguments[1]);

                    foreach (var item in ExpressionsAux)
                    {
                        if (item is MemberExpression member)
                            Expressions.Add(member);
                        else
                            ProcessExpression(item);
                    }
                }
                else if (exp is LambdaExpression lambda)
                    ProcessExpression(lambda.Body);
                else if (exp is BinaryExpression binaryExpression)
                    Expressions.Add(binaryExpression);
                else if (exp is MemberExpression member)
                    Expressions.Add(member);
            }

            ProcessExpression(expression.Body);

            return Expressions;
        }

        var expressions = SplitThenJoin(predicate);

        var translatorJoin = new JoinTranslator(typeof(T));
        var translatorWhere = new WhereTranslator(typeof(T));

        var joinsList = new List<JoinEntity>();
        var whereListSubEntities = new List<WhereModelQuery>();
        var entity = "";

        foreach (var expression in expressions)
        {
            if (expression is MemberExpression member)
            {
                var join = new JoinEntity();
                join = translatorJoin.TranslateExpression(member);
                entity = join.Entity;
                //joinsList.Add(join);
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                var where = new WhereModelQuery();
                where.Entity = entity;
                var whereInfo = translatorWhere.TranslateWhereExpression(binaryExpression);
                where.WhereList.Add(whereInfo.WhereScript);
                whereListSubEntities.Add(where);

                whereInfo.Entities.ForEach(entity =>
                {
                    if (!joinsList.Exists(e => e.Entity == entity))
                    {
                        var join = new JoinEntity { Entity = entity, MainEntity = where.Entity };
                        joinsList.Add(join);
                    }
                });
            }
        }

        var expressinsMembers = expressions.FindAll(e => e is MemberExpression member);

        joinsList.AddRange(translatorJoin.TranslateExpressions(expressinsMembers));

        joinsList.ForEach(join =>
        {
            var existInJoinList = stORMCore._config.JoinsList.Exists(joinExists => joinExists.Entity == join.Entity && joinExists.Name == join.Name);

            if (existInJoinList is false)
            {
                stORMCore._config.JoinsList.Add(join);
            }
            else
            {
                stORMCore._config.JoinsList = stORMCore._config.JoinsList.FindAll(joinEntity => joinEntity.Entity != join.Entity);
                stORMCore._config.JoinsList.Add(join);
            }
        });

        stORMCore._config.SubEntitiesWhereList.AddRange(whereListSubEntities);

        return this;
    }

    public virtual DbRepository<T> Join<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> predicate)
    {
        setConfigInORM();

        static List<Expression> SplitThenJoin<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var Expressions = new List<Expression>();

            void ProcessExpression(Expression exp)
            {
                if (exp is MethodCallExpression methodCall && (methodCall.Method.Name == "ThenJoin" || methodCall.Method.Name == "Where"))
                {
                    var ExpressionsAux = new List<Expression>();

                    ExpressionsAux.Add(methodCall.Arguments[0]);
                    ExpressionsAux.Add(methodCall.Arguments[1]);

                    foreach (var item in ExpressionsAux)
                    {
                        if (item is MemberExpression member)
                            Expressions.Add(member);
                        else
                            ProcessExpression(item);
                    }
                }
                else if (exp is LambdaExpression lambda)
                    ProcessExpression(lambda.Body);
                else if (exp is BinaryExpression binaryExpression)
                    Expressions.Add(binaryExpression);
                else if (exp is MemberExpression member)
                    Expressions.Add(member);
            }

            ProcessExpression(expression.Body);

            return Expressions;
        }

        var expressions = SplitThenJoin(predicate);

        var translatorJoin = new JoinTranslator(typeof(T));
        var translatorWhere = new WhereTranslator(typeof(T));

        var joinsList = new List<JoinEntity>();
        var whereListSubEntities = new List<WhereModelQuery>();
        var entity = "";

        foreach (var expression in expressions)
        {
            if (expression is MemberExpression member)
            {
                var join = new JoinEntity();
                join = translatorJoin.TranslateExpression(member);
                entity = join.Entity;
                //joinsList.Add(join);
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                var where = new WhereModelQuery();
                where.Entity = entity;
                var whereInfo = translatorWhere.TranslateWhereExpression(binaryExpression);
                where.WhereList.Add(whereInfo.WhereScript);
                whereListSubEntities.Add(where);

                whereInfo.Entities.ForEach(entity =>
                {
                    if (!joinsList.Exists(e => e.Entity == entity))
                    {
                        var join = new JoinEntity { Entity = entity, MainEntity = where.Entity };
                        joinsList.Add(join);
                    }
                });
            }
        }

        var expressinsMembers = expressions.FindAll(e => e is MemberExpression member);

        joinsList.AddRange(translatorJoin.TranslateExpressions(expressinsMembers));

        joinsList.ForEach(join =>
        {
            var existInJoinList = stORMCore._config.JoinsList.Exists(joinExists => joinExists.Entity == join.Entity && joinExists.Name == join.Name);

            if (existInJoinList is false)
            {
                stORMCore._config.JoinsList.Add(join);
            }
            else
            {
                stORMCore._config.JoinsList = stORMCore._config.JoinsList.FindAll(joinEntity => joinEntity.Entity != join.Entity);
                stORMCore._config.JoinsList.Add(join);
            }
        });

        stORMCore._config.SubEntitiesWhereList.AddRange(whereListSubEntities);

        return this;
    }

    /// <summary>
    /// Join with order By Desc, this method will help you to make the principal select with sub select and Order by DESC on sub select.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TInner"></typeparam>
    /// <param name="dataTable"></param>
    /// <param name="innerSelector"></param>
    /// <returns></returns>
    public virtual DbRepository<T> Join<TInner>(Expression<Func<T, IOrderedEnumerable<TInner>>> innerSelector)
    {
        setConfigInORM();

        var te = ExtractInnerSelector(innerSelector);
        var tes = ExtractOrderByPredicate(innerSelector);

        var translator = new OrderByTranslator(typeof(T), tes);

        var orderBy = translator.GenerateOrderBy();
        orderBy.Direction = "DESC";
        stORMCore._config.OrderByList.Add(orderBy);

        var translatorJoin = new JoinTranslator(typeof(T), te);
        var join = translatorJoin.TranslateExpression();

        var existInJoinList = stORMCore._config.JoinsList
            .Exists(joinExists => joinExists.Entity == join.Entity);

        if (existInJoinList is false)
        {
            stORMCore._config.JoinsList.Add(join);
        }
        else
        {
            stORMCore._config.JoinsList = stORMCore._config.JoinsList.FindAll(joinEntity => joinEntity.Entity != join.Entity);
            stORMCore._config.JoinsList.Add(join);
        }

        return this;
    }

    //public virtual DbRepository<T> Join<TInner>(Expression<Func<T, IEnumerable<TInner>>> innerSelector)
    //{
    //    setConfigInORM();

    //    var methodName = ExtractMethod(innerSelector);

    //    var te = ExtractInnerSelector(innerSelector);
    //    var tes = ExtractWherePredicate(innerSelector);

    //    if (methodName == "ThenJoin")
    //    {
    //        var translatorJoin = new JoinTranslator(typeof(T), te);
    //        var join = translatorJoin.TranslateExpression();

    //        var existInJoinList = stORMCore._config.JoinsList.Exists(joinExists => joinExists.Entity == join.Entity);

    //        if (existInJoinList is false)
    //        {
    //            stORMCore._config.JoinsList.Add(join);
    //        }
    //        else
    //        {
    //            stORMCore._config.JoinsList = stORMCore._config.JoinsList.FindAll(joinEntity => joinEntity.Entity != join.Entity);
    //            stORMCore._config.JoinsList.Add(join);
    //        }

    //        var translatorJoin2 = new JoinTranslator(typeof(T), tes);
    //        var join2 = translatorJoin2.TranslateExpression();

    //        var existInJoinList2 = stORMCore._config.JoinsList.Exists(joinExists => joinExists.Entity == join2.Entity);

    //        if (existInJoinList2 is false)
    //        {
    //            stORMCore._config.JoinsList.Add(join2);
    //        }
    //        else
    //        {
    //            stORMCore._config.JoinsList = stORMCore._config.JoinsList.FindAll(joinEntity => joinEntity.Entity != join2.Entity);
    //            stORMCore._config.JoinsList.Add(join2);
    //        }
    //    }

    //    if (methodName == "Where")
    //    {
    //        var translator = new WhereTranslator(typeof(TInner));
    //        var whereInfo = translator.TranslateWhereExpression(tes);

    //        var whereModelQuery = new WhereModelQuery();
    //        whereModelQuery.Entity = typeof(TInner).Name;

    //        var entities = whereInfo.Entities;
    //        whereModelQuery.WhereList.Add(whereInfo.WhereScript);

    //        stORMCore._config.SubEntitiesWhereList.Add(whereModelQuery);

    //        var existInJoinList = false;

    //        foreach (var entity in entities)
    //        {
    //            if (entity != typeof(T).Name)
    //            {
    //                existInJoinList = stORMCore._config.JoinsList.Exists(join => join.Entity == entity);
    //                if (existInJoinList is false)
    //                {
    //                    var joinEntity = new JoinEntity
    //                    {
    //                        Entity = entity
    //                    };
    //                    stORMCore._config.JoinsList.Add(joinEntity);
    //                }
    //            }
    //        }

    //        var translatorJoin = new JoinTranslator(typeof(T), te);
    //        var join = translatorJoin.TranslateExpression();


    //        existInJoinList = stORMCore._config.JoinsList
    //            .Exists(joinExists => joinExists.Entity == join.Entity);

    //        if (existInJoinList is false)
    //        {
    //            stORMCore._config.JoinsList.Add(join);
    //        }
    //        else
    //        {
    //            stORMCore._config.JoinsList = stORMCore._config.JoinsList
    //                            .FindAll(joinEntity => joinEntity.Entity != join.Entity);

    //            stORMCore._config.JoinsList.Add(join);
    //        }
    //    }

    //    return this;
    //}

    public virtual DbRepository<T> AND(Expression<Func<T, bool>> predicate)
    {
        setConfigInORM();

        var translator = new AndTranslator(typeof(T), predicate.Body);
        var joinAnd = translator.TranslateExpression();

        var join = stORMCore._config.JoinsList.Last();
        if (join.Entity == joinAnd.Entity)
        {
            join.JoinAnd = joinAnd;
        }

        return this;
    }

    public virtual DbRepository<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> predicate)
    {
        setConfigInORM();

        var translator = new OrderByTranslator(typeof(T), predicate.Body);

        var orderBy = translator.GenerateOrderBy();
        stORMCore._config.OrderByList.Add(orderBy);
        return this;
    }

    public virtual DbRepository<T> Desc()
    {
        setConfigInORM();

        var orderBy = stORMCore._config.OrderByList.Last();
        orderBy.Direction = "Desc";

        return this;
    }

    public virtual DbRepository<T> Skip(int pageNumber)
    {
        setConfigInORM();

        var pagination = new Pagination();
        pagination.HasPagination = true;
        pagination.Page = pageNumber;

        stORMCore._config.Pagination = pagination;
        return this;
    }

    public virtual DbRepository<T> Take(int pageSize)
    {
        setConfigInORM();

        stORMCore._config.Pagination.PageSize = pageSize;
        return this;
    }

    public virtual DbRepository<T> Case(Expression<Func<T, bool>> predicate)
    {
        setConfigInORM();

        var translator = new CaseTranslator(typeof(T), predicate.Body);

        var whenModel = translator.TranslateExpression();

        stORMCore._config.Case.Whens.Add(whenModel);
        stORMCore._config.Case.Entity = whenModel.Entity;
        stORMCore._config.Case.EntityAlias = whenModel.EntityAlias;
        stORMCore._config.Case.EntityProp = whenModel.EntityProp;

        return this;
    }
    public virtual DbRepository<T> Then(int value)
    {
        setConfigInORM();

        var whenModel = stORMCore._config.Case.Whens.Last();
        whenModel.ThenValue = value.ToString();

        return this;
    }

    public virtual DbRepository<T> Then(string value)
    {
        setConfigInORM();

        var whenModel = stORMCore._config.Case.Whens.Last();
        whenModel.ThenValue = $" '{value} ' ";

        return this;
    }

    public virtual DbRepository<T> Else(int value)
    {
        setConfigInORM();

        stORMCore._config.Case.Else = value.ToString();
        return this;
    }

    public virtual DbRepository<T> Else(string value)
    {
        setConfigInORM();

        stORMCore._config.Case.Else = $" '{value} ' ";
        return this;
    }

    public virtual DbRepository<T> End(string value)
    {
        setConfigInORM();

        stORMCore._config.Case.End = value.Replace(" ", "");
        stORMCore._config.Cases.Add(stORMCore._config.Case);
        stORMCore._config.Case = new CaseModel();
        return this;
    }

    private Expression ExtractWherePredicate<T, TInner>(
       Expression<Func<T, IEnumerable<TInner>>> joinExpression)
    {
        if (joinExpression.Body is MethodCallExpression methodCall)
        {
            // O primeiro argumento do método Where é o predicate
            var predicate = (LambdaExpression)methodCall.Arguments[1];
            return predicate.Body;
        }

        throw new InvalidOperationException("A expressão não contém um método Where.");
    }

    private Expression ExtractOrderByPredicate<T, TInner>(Expression<Func<T, IOrderedEnumerable<TInner>>> joinExpression)
    {
        if (joinExpression.Body is MethodCallExpression methodCall &&
            methodCall.Method.Name == "OrderByDescending")
        {
            // O primeiro argumento do método Where é o predicate
            var predicate = (LambdaExpression)methodCall.Arguments[1];
            return predicate.Body;
        }

        throw new InvalidOperationException("A expressão não contém um método Where.");
    }

    private string ExtractMethod<T, TInner>(
        Expression<Func<T, IEnumerable<TInner>>> joinExpression)
    {
        if (joinExpression.Body is MethodCallExpression methodCall) return methodCall.Method.Name;

        return "";
    }

    private Expression ExtractInnerSelector<T, TInner>(
        Expression<Func<T, IEnumerable<TInner>>> joinExpression)
    {

        if (joinExpression.Body is MethodCallExpression methodCall)
        {
            // O primeiro argumento do método Where é o predicate
            var predicate = (MemberExpression)methodCall.Arguments[0];
            return predicate;
        }


        throw new InvalidOperationException("A expressão não contém um método Where.");
    }

    private Expression ExtractInnerSelector<T, TInner>(
        Expression<Func<T, IOrderedEnumerable<TInner>>> joinExpression)
    {
        if (joinExpression.Body is MethodCallExpression methodCall &&
            methodCall.Method.Name == "OrderByDescending")
        {
            // O primeiro argumento do método Where é o predicate
            var predicate = (MemberExpression)methodCall.Arguments[0];
            return predicate;
        }

        throw new InvalidOperationException("A expressão não contém um método Where.");
    }

    public virtual int Insert(T entity)
    {
        setConfigInORM();

        var result = stORMCore.Generate<InsertGen>(entity);

        if (result == null)
            return 0;

        int idCreated = (int)result;

        return idCreated;
    }


    public virtual int Update(T entity)
    {
        setConfigInORM();
        var result = stORMCore.Generate<UpdateGen>(entity);

        if (result == null)
            return 0;

        int idCreated = (int)result;

        return idCreated;
    }


    public virtual int Delete(T entity)
    {
        setConfigInORM();
        var result = stORMCore.Generate<DeleteGen>(entity);

        if (result == null)
            return 0;

        int idCreated = (int)result;

        return idCreated;

    }

    public virtual int SaveChanges()
    {
        try
        {
            stORMCore.SaveChanges();
            return 1;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }
}
