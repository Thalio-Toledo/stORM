using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static stORM.Models.GroupByModel;

namespace confirp_bonescore.BonesCoreOrm.ExpressionsTranslators;

public class ExistsTranslator
{
    public Type _entity;
    public Expression _expression;
    public ExistsModel where = new ExistsModel();
    public ExistsTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
    }

    public ExistsModel TranslateExpression()
    {
        
        // Pega o corpo da expressão (neste caso, result <= 1)
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);

            // Pega o operador (por exemplo, LessThanOrEqual, GreaterThan, etc.)
            where.SqlOperator = GetSqlOperator(binaryExpression.NodeType);

            // Pega o valor à direita (neste caso, 1)

            if (binaryExpression.Right is ConstantExpression constantExpression)
            {
                if (constantExpression.Value is null)
                {
                    where.SqlOperator = "IS";
                    where.Value = "NULL";
                }
                else
                {
                    where.Value = ((ConstantExpression)binaryExpression.Right).Value.ToString();
                }
            }
        }

        if (_expression is MemberExpression memberExpression)
        {
            Type memberType = ((PropertyInfo)memberExpression.Member).PropertyType;

            // Verificar se o tipo é uma coleção genérica (ex: List<T>)
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                where.Entity = memberType.GetGenericArguments()[0].Name;
            }
            else
            {
                where.Entity = memberExpression.Type.Name;
            }

            where.MainEntity = memberExpression.Expression.Type.Name;
        }

        if (_expression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression2)
            {
                Type memberType = ((PropertyInfo)memberExpression2.Member).PropertyType;

                // Verificar se o tipo é uma coleção genérica (ex: List<T>)
                if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // Retorna o tipo genérico, ou seja, o tipo 'T' de List<T>
                    where.Entity = memberType.GetGenericArguments()[0].Name;
                }
                else
                {
                    where.Entity = memberExpression2.Type.Name;
                }

                where.MainEntity = memberExpression2.Expression.Type.Name;
            }
        }

        where.FunctionName = "EXISTS";


        return where;
    }

    private static string GetSqlOperator(ExpressionType expressionType)
    {
        // Mapear os operadores C# para operadores SQL
        return expressionType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.AndAlso => "AND",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.NotEqual => "!=",
            _ => throw new NotSupportedException($"Operador '{expressionType}' não é suportado.")
        };
    }

    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            where.Entity = memberExpression.Expression.Type.Name;
        }
    }

}
