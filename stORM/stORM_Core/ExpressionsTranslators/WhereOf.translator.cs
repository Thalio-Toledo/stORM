using System.Linq.Expressions;
using static stORM.Models.GroupByModel;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators
{
    public class WhereOfTranslator
    {
        public Type _entity;
        public Expression _expression;
        public WhereModel where = new WhereModel();
        public WhereOfTranslator(Type T, Expression expression)
        {
            _entity = T;
            _expression = expression;
        }

        public WhereModel TranslateExpression()
        {
            if (_expression is BinaryExpression binaryExpression)
            {
                GetLeftExpression(binaryExpression.Left);
            }

            if (_expression is MemberExpression memberExpression)
            {
                where.Entity = memberExpression.Expression.Type.Name;
                where.EntityProp = memberExpression.Member.Name;
            }

            return where;

            throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
        }
        private void GetLeftExpression(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                where.Entity = memberExpression.Expression.Type.Name;
                where.EntityProp = memberExpression.Member.Name;
            }
        }
    }

    //private static string TranslateExpression(Expression expression)
    //{
    //    if (expression is BinaryExpression binaryExpression)
    //    {
    //        // Traduz expressões binárias como id == 1
    //        var left = TranslateExpression(binaryExpression.Left);
    //        var right = TranslateExpression(binaryExpression.Right);
    //        var @Operator = GetSqlOperator(binaryExpression.NodeType);
    //        return $"{left} {@Operator} {right}";
    //    }
    //    else if (expression is MemberExpression memberExpression)
    //    {
    //        return memberExpression.ToString();
    //        // Traduz o membro (por exemplo, table.Id)
    //        return memberExpression.Member.Name;
    //    }
    //    else if (expression is ConstantExpression constantExpression)
    //    {
    //        // Traduz constantes (por exemplo, 1)
    //        return constantExpression.Value.ToString();
    //    }

    //    throw new NotSupportedException($"O tipo de expressão '{expression.GetType()}' não é suportado.");
    //}

    //private static string GetSqlOperator(ExpressionType expressionType)
    //{
    //    // Mapear os operadores C# para operadores SQL
    //    return expressionType switch
    //    {
    //        ExpressionType.Equal => "=",
    //        ExpressionType.GreaterThan => ">",
    //        ExpressionType.LessThan => "<",
    //        ExpressionType.AndAlso => "AND",
    //        _ => throw new NotSupportedException($"Operador '{expressionType}' não é suportado.")
    //    };
    //}
}

