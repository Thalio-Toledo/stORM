using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static stORM.Models.GroupByModel;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators;

public class CaseTranslator
{
    
    public Type _entity;
    public Expression _expression;
    public WhenModel When = new WhenModel();
    public CaseTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
        
    }

    public WhenModel TranslateExpression()
    {   
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);

            When.Operator = GetSqlOperator(binaryExpression.NodeType);

            if (When.Entity is null)
            {
                var unaryExpression = (UnaryExpression)binaryExpression.Left;

                // Extraindo o valor da expressão subjacente (processo.Id)
                var memberExpression = (MemberExpression)unaryExpression.Operand;
                GetLeftExpression(memberExpression);
            }

            var rightExpression = binaryExpression.Right;

            // Verificamos se é uma MemberExpression e pegamos o valor
            if (rightExpression is MemberExpression memberExpr)
            {
                try
                {
                    // Extrai o valor da variável capturada
                    var capturedValue = GetCapturedVariableValue(memberExpr);
                    When.WhenValue = capturedValue.ToString();
                }
                catch (Exception ex)
                {
                    // Extrai o valor da variável capturada
                    var capturedValue = GetMemberValue(memberExpr);
                    When.WhenValue = capturedValue.ToString();
                }

            }
            else
            {
                GetRightExpression(binaryExpression.Right);
            }

        }

        return When;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }
    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            When.Entity = memberExpression.Expression.Type.Name;
            When.EntityProp = memberExpression.Member.Name;
        }
    }

    private void GetRightExpression(Expression expression)
    {

        if (expression is ConstantExpression constantExpression)
        {
            if(constantExpression.Value is null)
            {
                When.WhenValue = null;
            }
            else
            {
                var typeValue = constantExpression.Value.GetType();

                if (constantExpression.Value is int)
                {
                    When.WhenValue = constantExpression.Value.ToString();
                }
                else if (constantExpression.Value is bool)
                {

                    if ((bool)constantExpression.Value)
                    {
                        When.WhenValue = "1";
                    }
                    else
                    {
                        When.WhenValue = "0";
                    }
                }
                else
                {
                    When.WhenValue = constantExpression.Value.ToString();
                }
            }
        }
    }
    public object GetCapturedVariableValue(MemberExpression memberExpression)
    {
        // A expressão 'memberExpression.Expression' contém o fechamento (closure)
        var closureExpression = (ConstantExpression)memberExpression.Expression;

        // A classe gerada pelo compilador (<>c__DisplayClass) contém o valor da variável capturada
        var closureObject = closureExpression.Value;

        // Usamos reflexão para obter o valor do campo 'testeNumero' a partir do fechamento
        var fieldInfo = (FieldInfo)memberExpression.Member;
        return fieldInfo.GetValue(closureObject);
    }

    public object GetMemberValue(MemberExpression memberExpression)
    {
        // Extrair o objeto pai (que é a closure capturada, por exemplo <>c__DisplayClass0_0)
        if (memberExpression.Expression is MemberExpression innerMember)
        {
            // Pegue o objeto 'filter' que está dentro da closure
            var closure = (ConstantExpression)innerMember.Expression;
            var closureObject = closure.Value;

            // Pegue o campo 'filter' da closure usando reflexão
            var fieldInfo = (FieldInfo)innerMember.Member;
            var filterObject = fieldInfo.GetValue(closureObject);

            // Agora, pegue o campo 'Id' do objeto 'filter'
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return propertyInfo.GetValue(filterObject);
        }

        return null;
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

