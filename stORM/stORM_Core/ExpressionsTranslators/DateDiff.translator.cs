using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static stORM.Models.GroupByModel;

namespace confirp_bonescore.BonesCoreOrm.ExpressionsTranslators;

public class DateDiffTranslator
{
    public Type _entity;
    public Expression _expression;
    public DateDiffDay where = new DateDiffDay();
    public DateDiffTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
    }

    public DateDiffDay TranslateExpression()
    {
        // Pega o corpo da expressão (neste caso, result <= 1)
        if (_expression is BinaryExpression binaryExpression)
        {
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

        where.FunctionName = "DATEDIFF";


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

}
