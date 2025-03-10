using System.Linq.Expressions;
using System.Reflection;
using static stORM.Models.GroupByModel;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators;

public class CountOfTranslator
{

    public Type _entity;
    public Expression _expression;
    public CountModel CountEntity = new CountModel();
    public CountOfTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
    }

    public CountModel TranslateExpression()
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
                CountEntity.Entity = memberType.GetGenericArguments()[0].Name;
            }
            else
            {
                CountEntity.Entity = memberExpression.Type.Name;
            }

        }

        return CountEntity;


        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }
    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            CountEntity.Entity = memberExpression.Expression.Type.Name;
        }
    }
}
