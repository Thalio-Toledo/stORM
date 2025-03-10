using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static stORM.Models.GroupByModel;

namespace confirp_bonescore.BonesCoreOrm.ExpressionsTranslators;

public class FirstOrDefaultOfTranslator
{
    public Type _entity;
    public Expression _expression;
    public TopModelOf topModelOf = new TopModelOf();
    public FirstOrDefaultOfTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
    }

    public TopModelOf TranslateExpression()
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
                topModelOf.Entity = memberType.GetGenericArguments()[0].Name;
            }
            else
            {
                topModelOf.Entity = memberExpression.Type.Name;
            }
        }

        return topModelOf;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }
    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            topModelOf.Entity = memberExpression.Expression.Type.Name;
        }
    }
}
