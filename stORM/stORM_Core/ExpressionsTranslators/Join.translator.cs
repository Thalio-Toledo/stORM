using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using stORM.Models;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators;

public class JoinTranslator
{
    public Type _entity; 
    public Expression _expression;
    public JoinEntity JoinEntity = new JoinEntity();
    public JoinTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
        JoinEntity.JoinFull = true;
    }

    public JoinTranslator(Type T)
    {
        _entity = T;
    }

    public JoinEntity TranslateExpression()
    {
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left); 
        }


        if (_expression is MemberExpression memberExpression)
        {
            Type memberType = ((PropertyInfo)memberExpression.Member).PropertyType;

            // Verificar se o tipo é uma coleção genérica (ex: List<T>)
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                JoinEntity.Entity = memberType.GetGenericArguments()[0].Name;
            }
            else
            {
                JoinEntity.Entity = memberExpression.Type.Name;
            }

            JoinEntity.Name = memberExpression.Member.Name;
            JoinEntity.MainEntity = memberExpression.Expression.Type.Name;
            JoinEntity.FullInheritance = GetFullInheritance(_expression);
        }
        
        if(_expression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression2)
            {
                Type memberType = ((PropertyInfo)memberExpression2.Member).PropertyType;

                // Verificar se o tipo é uma coleção genérica (ex: List<T>)
                if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                    JoinEntity.Entity = memberType.GetGenericArguments()[0].Name;
                }
                else
                {
                    JoinEntity.Entity = memberExpression2.Type.Name;
                }

                JoinEntity.MainEntity = memberExpression2.Expression.Type.Name;
            }
  
        }
        

        return JoinEntity;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }

    public JoinEntity TranslateExpression(Expression expression)
    {
        var joinEntity = new JoinEntity();
        joinEntity.JoinFull = true;

        if (expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
        }

        if (expression is MemberExpression memberExpression)
        {
            Type memberType = ((PropertyInfo)memberExpression.Member).PropertyType;

            // Verificar se o tipo é uma coleção genérica (ex: List<T>)
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                joinEntity.Entity = memberType.GetGenericArguments()[0].Name;
            }
            else
            {
                joinEntity.Entity = memberExpression.Type.Name;
            }

            joinEntity.Name = memberExpression.Member.Name;
            joinEntity.MainEntity = memberExpression.Expression.Type.Name;
            joinEntity.FullInheritance = GetFullInheritance(expression);

            if (joinEntity.MainEntity != _entity.Name)
            {
                joinEntity.FullInheritance = GetFullInheritance(expression);
            }
            else
            {
                joinEntity.FullInheritance = GetFullInheritance(expression);
            }
        }

        if (expression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression2)
            {
                Type memberType = ((PropertyInfo)memberExpression2.Member).PropertyType;

                // Verificar se o tipo é uma coleção genérica (ex: List<T>)
                if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                    joinEntity.Entity = memberType.GetGenericArguments()[0].Name;
                }
                else
                {
                    joinEntity.Entity = memberExpression2.Type.Name;
                }

                joinEntity.MainEntity = memberExpression2.Expression.Type.Name;
            }
        }



        return joinEntity;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }

    public List<JoinEntity> TranslateExpressions(List<Expression> expressions)
    {
        var Joins = new List<JoinEntity>();

        expressions.ForEach(expression =>
        {
            var join = new JoinEntity();
            join.JoinFull = true;
            if (expression is BinaryExpression binaryExpression)
            {
                GetLeftExpression(binaryExpression.Left);
            }

            if (expression is MemberExpression memberExpression)
            {
                Type memberType = ((PropertyInfo)memberExpression.Member).PropertyType;

                // Verificar se o tipo é uma coleção genérica (ex: List<T>)
                if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                    join.Entity = memberType.GetGenericArguments()[0].Name;
                }
                else
                {
                    join.Entity = memberExpression.Type.Name;
                }

                join.Name = memberExpression.Member.Name;
                join.MainEntity = memberExpression.Expression.Type.Name;

                if(join.MainEntity != _entity.Name)
                {
                    join.FullInheritance = GetFullInheritance(expressions);
                }
                else
                {
                    join.FullInheritance = GetFullInheritance(expression);
                }
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression.Body is MemberExpression memberExpression2)
                {
                    Type memberType = ((PropertyInfo)memberExpression2.Member).PropertyType;

                    // Verificar se o tipo é uma coleção genérica (ex: List<T>)
                    if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                        join.Entity = memberType.GetGenericArguments()[0].Name;
                    }
                    else
                    {
                        join.Entity = memberExpression2.Type.Name;
                    }

                    join.MainEntity = memberExpression2.Expression.Type.Name;
                }
            }
            Joins.Add(join);

        });

        return Joins;
    }

    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            JoinEntity.Entity = memberExpression.Expression.Type.Name;
        }
    }

    private string GetFullInheritance(Expression expression)
    {
        var entities = new List<string>();

        while (expression is MemberExpression memberExpressions)
        {
            entities.Add(memberExpressions.Member.Name);
            expression = memberExpressions.Expression;
        }
        entities.Reverse();

        return string.Join(".", entities);
    }

    private string GetFullInheritance(List<Expression> expressions)
    {
        var entities = new List<string>();
        expressions.ForEach(expression =>
        {

            while (expression is MemberExpression memberExpressions)
            {
                entities.Add(memberExpressions.Member.Name);
                expression = memberExpressions.Expression;
            }

        });

        //if (entities.Count > 3)
        //{
        //    entities.RemoveAt(3);
        //}

        return string.Join(".", entities);
    }

}
