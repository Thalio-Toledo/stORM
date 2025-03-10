using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using stORM.Models;
using static stORM.Models.GroupByModel;

namespace confirp_bonescore.BonesCoreOrm.ExpressionsTranslators;

public class AndTranslator
{
    public Type _entity;
    public Expression _expression;
    public JoinAndModel joinAnd = new JoinAndModel();
    public AndTranslator(Type T, Expression expression)
    {
        _entity = T;
        _expression = expression;
    }


    public JoinAndModel TranslateExpression()
    {
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
            joinAnd.SqlOperator = GetSqlOperator(binaryExpression.NodeType);

            if (joinAnd.Entity is null)
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
                    var capturedValue = GetMemberValue(memberExpr);

                    if (capturedValue is not null)
                    {
                        setValueByType(capturedValue);
                    }
                    else
                    {
                        capturedValue = GetCapturedVariableValue(memberExpr);

                        setValueByType(capturedValue);

                    }

                }
                catch (Exception ex)
                {
                    // Extrai o valor da variável capturada
                    var capturedValue = GetCapturedVariableValue(memberExpr);

                    setValueByType(capturedValue);
                }

            }
            else
            {
                if (HasNullableType(binaryExpression.Right))
                {
                    var capturedValue = GetValueNullable(binaryExpression.Right);
                    setValueByType(capturedValue);
                }
                else
                {
                    var capturedValue = GetRightExpression(binaryExpression.Right);
                    if (capturedValue is null)
                        capturedValue = GetCapturedVariableValue(binaryExpression.Right);

                    setValueByType(capturedValue);
                }

            }

        }

        return joinAnd;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }
    private void GetLeftExpression(Expression expression)
    {


        if (expression is MemberExpression memberExpression)
        {
            joinAnd.Entity = memberExpression.Expression.Type.Name;
            joinAnd.EntityProp = memberExpression.Member.Name;

            //if (memberExpression.Expression is MemberExpression memberExpression2)
            //{
            //    joinAnd.MainEntity = memberExpression2.Member.ReflectedType.Name;
            //}

        }

    }

    private object GetRightExpression(Expression expression)
    {

        if (expression is ConstantExpression constantExpression)
        {
            return constantExpression.Value;

        }

        return null;


    }
    public object GetCapturedVariableValue(MemberExpression memberExpression)
    {
        if (memberExpression.Expression is ConstantExpression constantExpression2)
        {
            // A expressão 'memberExpression.Expression' contém o fechamento (closure)
            var closureExpression = (ConstantExpression)memberExpression.Expression;

            // A classe gerada pelo compilador (<>c__DisplayClass) contém o valor da variável capturada
            var closureObject = closureExpression.Value;

            // Usamos reflexão para obter o valor do campo 'testeNumero' a partir do fechamento
            var fieldInfo = (FieldInfo)memberExpression.Member;
            return fieldInfo.GetValue(closureObject);
        }
        else
        {
            return null;
        }


    }

    private object GetCapturedVariableValueFromFilter(Expression expression)
    {

        // O UnaryExpression contém um MemberExpression (ex: controller.filter.IdEmpresa)
        if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Expression is ConstantExpression constantExpression)
            {
                // Pega a instância da classe gerada (<>c__DisplayClass)
                var capturedObject = constantExpression.Value;

                // Usa reflexão para acessar o campo 'filter' dentro da classe capturada
                var filterField = capturedObject.GetType().GetField("filter", BindingFlags.Instance | BindingFlags.Public);
                if (filterField != null)
                {
                    // Pega a instância de 'filter'
                    var filterInstance = filterField.GetValue(capturedObject);

                    // Usa reflexão para acessar a propriedade 'IdEmpresa' dentro de 'filter'
                    var idEmpresaProperty = filterInstance.GetType().GetProperty("IdEmpresa");
                    return idEmpresaProperty?.GetValue(filterInstance);
                }
            }

            ExploreExpression(expression);

            // Verifica se a expressão que contém o objeto capturado é ConstantExpression
            GetCapturedVariableValue(memberExpression);
        }

        return null;
    }

    public MemberExpression MyProperty1 { get; set; }
    public MemberExpression MyProperty2 { get; set; }
    private void ExploreExpression(Expression expression)
    {
        Console.WriteLine($"Tipo de expressão: {expression.GetType().Name}");

        switch (expression)
        {
            case UnaryExpression unaryExpression:
                Console.WriteLine("UnaryExpression encontrada (Convert)");
                ExploreExpression(unaryExpression.Operand); // Continua explorando o operando
                break;

            case MemberExpression memberExpression:
                Console.WriteLine($"MemberExpression encontrada: {memberExpression.Member.Name}");
                this.MyProperty1 = memberExpression;
                this.MyProperty2 = MyProperty1;
                ExploreExpression(memberExpression.Expression); // Explora a expressão interna
                break;

            case ConstantExpression constantExpression:
                Console.WriteLine($"ConstantExpression encontrada com valor: {constantExpression.Value}");
                var closureExpression = constantExpression;

                // A classe gerada pelo compilador (<>c__DisplayClass) contém o valor da variável capturada
                var closureObject = closureExpression.Value;
                var teste = closureObject.GetType().GetField(MyProperty1.Member.Name, BindingFlags.Instance | BindingFlags.Public);
                var teste2 = teste.GetValue(closureObject);
                // Usamos reflexão para obter o valor do campo 'testeNumero' a partir do fechamento
                break;

            default:
                Console.WriteLine($"Outro tipo de expressão encontrado: {expression.GetType().Name}");
                break;
        }
    }

    public object GetValueNullable(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            // Pega o valor da expressão original (antes da conversão)
            if (unaryExpression.Operand is ConstantExpression constantExpression)
            {
                return constantExpression.Value;
            }

            // O UnaryExpression tem um MemberExpression dentro (controller.id)
            if (unaryExpression.Operand is MemberExpression memberExpression)
            {
                // Extrai a ConstantExpression que contém a instância do objeto controller
                if (memberExpression.Expression is ConstantExpression constantExpression2)
                {
                    // Pega a instância da classe que contém a variável capturada
                    var capturedObject = constantExpression2.Value;

                    // Usa reflexão para acessar o campo 'id' no objeto capturado
                    var field = capturedObject.GetType().GetField(memberExpression.Member.Name, BindingFlags.Instance | BindingFlags.Public);
                    return field?.GetValue(capturedObject);
                }
            }


        }

        return null;

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

    private void SetWhereValue(string value)
    {
        value.Trim();
        var stringValueArray = value.Split(" ");

        StringBuilder sb = new StringBuilder();

        foreach (var item in stringValueArray)
        {
            if (item == stringValueArray[stringValueArray.Length - 1])
            {
                sb.Append($" '%{item}%' ");
            }
            else
            {
                sb.Append($" '%{item}%' ");
            }
        }

        joinAnd.Value = sb.ToString();
        joinAnd.SqlOperator = "LIKE";
    }

    private void SetWhereValue(bool value)
    {
        if (value)
        {
            joinAnd.Value = "1";
        }
        else
        {
            joinAnd.Value = "0";
        }
    }

    private void SetWhereValue(DateTime value)
    {
        var dateString = value.ToString("yyyy-MM-dd"); ;
        joinAnd.Value = $" '{dateString}' ";
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


    private bool HasNullableType(Expression expression)
    {
        // Verifica se o tipo da expressão é Nullable<T>
        if (expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return true;  // A expressão tem Nullable<T>
        }

        // Verifica se é uma BinaryExpression (como processo.Id == 10)
        if (expression is BinaryExpression binaryExpression)
        {
            // Verifica o lado esquerdo e direito da expressão
            return HasNullableType(binaryExpression.Left) || HasNullableType(binaryExpression.Right);
        }

        // Verifica se é uma MemberExpression (como processo.Id)
        if (expression is MemberExpression memberExpression)
        {
            return HasNullableType(memberExpression);
        }

        // Verifica se é uma UnaryExpression (que pode envolver conversão para Nullable)
        if (expression is UnaryExpression unaryExpression)
        {
            return HasNullableType(unaryExpression.Operand);
        }

        // Se nenhum dos casos acima for verdade, retorna false
        return false;
    }

    // Verifica se uma MemberExpression (ex: processo.Id) é Nullable<T>
    private bool HasNullableType(MemberExpression memberExpression)
    {
        // Verifica se o tipo do membro é Nullable<T>
        if (Nullable.GetUnderlyingType(memberExpression.Type) != null)
        {
            return true;
        }

        return false;
    }

    private void setValueByType(object valueObj)
    {
        if (valueObj is DateTime)
        {
            var value = (DateTime)valueObj;
            SetWhereValue(value);
        }
        if (valueObj is int)
        {
            joinAnd.Value = valueObj.ToString();
        }
        if (valueObj is string)
        {
            var value = valueObj.ToString();

            SetWhereValue(value);
        }

        if (valueObj is bool)
        {
            var value = (bool)valueObj;

            SetWhereValue(value);
        }
    }

    private object GetCapturedVariableValue(Expression expression)
    {
        // Verifica se a expressão é uma BinaryExpression (ex: processo.IdEmpresa == ...)

        // Verifica o lado direito da expressão (que contém a conversão e a variável capturada)
        if (expression is UnaryExpression unaryExpression)
        {

            if (unaryExpression.Operand is BinaryExpression binaryExpression)
            {

            }
            // O UnaryExpression contém um MemberExpression (ex: controller.idEmpresa)
            if (unaryExpression.Operand is MemberExpression memberExpression)
            {
                // Verifica se a expressão que contém o objeto capturado é ConstantExpression
                if (memberExpression.Expression is ConstantExpression constantExpression)
                {
                    // Pega a instância da classe gerada pelo compilador (<>c__DisplayClass)
                    var capturedObject = constantExpression.Value;

                    // Usa reflexão para acessar o campo 'idEmpresa' dentro da classe gerada
                    var field = capturedObject.GetType().GetField(memberExpression.Member.Name, BindingFlags.Instance | BindingFlags.Public);
                    return field?.GetValue(capturedObject);  // Retorna o valor de idEmpresa
                }
                else
                {
                    GetCapturedVariableValueFromFilter((MemberExpression)unaryExpression.Operand);
                }
            }
        }


        return null;
    }
}
