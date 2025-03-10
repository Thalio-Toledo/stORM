using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using stORM.utils;
using static stORM.Models.GroupByModel;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators;

public class SubExpressionExtractor : ExpressionVisitor
{
    private readonly string _targetExpressionString;
    private Expression _foundExpression;

    public SubExpressionExtractor(string targetExpressionString)
    {
        _targetExpressionString = targetExpressionString;
    }

    public Expression Extract(Expression expression)
    {
        _foundExpression = null;
        Visit(expression);
        return _foundExpression;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Converte o nó em string e compara com a string alvo
        if (node.ToString().Equals(_targetExpressionString, StringComparison.OrdinalIgnoreCase))
        {
            _foundExpression = node;
        }

        return base.VisitBinary(node);
    }

    // Outros tipos de expressões podem ser visitados se necessário
}

// Visitante de Expressão para buscar expressões binárias
public class BinaryExpressionExtractor : ExpressionVisitor
{
    private readonly string _searchString;
    public Expression FoundExpression { get; private set; }

    public BinaryExpressionExtractor(string searchString)
    {
        _searchString = searchString.Trim('(', ' ', ')');
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Monta a expressão exatamente como "p.Ativo == True"
        string left = node.Left.ToString();
        string right = node.Right.ToString();
        string op = GetOperator(node.NodeType); // Obtem o operador ==, &&, ||

        string binaryString = $"{left} {op} {right}";

        // Compara com a string buscada
        if (binaryString.Equals(_searchString, StringComparison.OrdinalIgnoreCase))
        {
            FoundExpression = node;
        }

        if(FoundExpression is null)
        {
            if (node.ToString() == _searchString)
            {
                FoundExpression = node;
            }
        }

        // Continua visitando outros nós até encontrar
        return base.VisitBinary(node);
    }

    // Função auxiliar para obter o operador correto
    private string GetOperator(ExpressionType type)
    {
        return type switch
        {
            ExpressionType.Equal => "==",
            ExpressionType.AndAlso => "&&",
            ExpressionType.OrElse => "||",
            _ => type.ToString()
        };
    }
}

public class WhereInfo
{
    public List<string> Entities { get; set; } = new List<string>();
    public string WhereScript { get; set; } 
}


public class WhereTranslator
{
    public Type _entity;
    public Expression _expression;
    public WhereModel where = new WhereModel();

    public WhereTranslator(Type T, Expression expression = null)
    {
        _entity =  T;
        _expression = expression;
    }

    public static Expression ExtractBinaryExpression(Expression expression, string searchString)
    {
        var visitor = new BinaryExpressionExtractor(searchString);
        visitor.Visit(expression);
        return visitor.FoundExpression;
    }

    public WhereInfo TranslateWhereExpression(Expression expression)
    {
        return decomposeExpression(expression);
    }

    private WhereInfo decomposeExpression(Expression expression)
    {
        var whereInfo = new WhereInfo();

        var whereList = new List<WhereModel>();

        var expString = expression.ToString();

        string pattern = @"(?<=AndAlso|OrElse)|(?=AndAlso|OrElse)";

        var exps = Regex.Split(expString, pattern);

        var expressoes = new List<string>();

        foreach (var expre in exps)
        {
            var operador = "";

            if (expre is "AndAlso" || expre is "OrElse")
            {
                operador = expre switch
                {
                    "AndAlso" => "AND",
                    "OrElse" => "OR",
                    _ => throw new NotSupportedException($"Operador '{expre}' não é suportado.")
                };

                expressoes.Add(operador);

                var whereOperator = new WhereModel();
                whereOperator.WhereExpression = operador;

                whereList.Add(whereOperator);
            }
            else
            {
                var open = new string(expre.Where(c => c == '(').ToArray());
                var close = new string(expre.Where(c => c == ')').ToArray());

                var extractedExpression = ExtractBinaryExpression(expression, expre);

                if(extractedExpression is null)
                {
                    
                    string pat = $@"\){"{" + (close.Count() -  open.Count()) + "}$"}";
                    string output = Regex.Replace(expre, pat, "");
                    var extractor = new SubExpressionExtractor(output.Trim());
                    extractedExpression = extractor.Extract(expression);
                }

                var whereExpression = TranslatorExpression(extractedExpression);

                whereExpression.WhereExpression = $"{open}{whereExpression.Entity}.{whereExpression.EntityProp} {whereExpression.SqlOperator} {whereExpression.Value} {close}";
                whereList.Add(whereExpression);

                whereInfo.Entities.Add(whereExpression.Entity);

                expressoes.Add($"{open}{UtilsService.GenerateAlias(whereExpression.Entity)}.{whereExpression.EntityProp} {whereExpression.SqlOperator} {whereExpression.Value} {close}");
            }
        }

         whereInfo.WhereScript = string.Join(" ", expressoes);

        return whereInfo;
    }

    public WhereModel TranslatorExpression(Expression expression)
    {
        where = new WhereModel();

        if (expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
            where.SqlOperator = GetSqlOperator(binaryExpression.NodeType);

            if (where.Entity is null)
            {
                var unaryExpression = (UnaryExpression)binaryExpression.Left;

                // Extraindo o valor da expressão subjacente (processo.Id)
                var memberExpression = (MemberExpression)unaryExpression.Operand;
                GetLeftExpression(memberExpression);
            }

            GetValueRightExpression(binaryExpression.Right);
            
        }

        return where;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }

    public WhereModel TranslateExpression()
    {
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
            where.SqlOperator = GetSqlOperator(binaryExpression.NodeType);

            if (where.Entity is null)
            {
                var unaryExpression = (UnaryExpression)binaryExpression.Left;

                // Extraindo o valor da expressão subjacente (processo.Id)
                var memberExpression = (MemberExpression)unaryExpression.Operand;
                GetLeftExpression(memberExpression);
            }

            GetValueRightExpression(binaryExpression.Right);
        }

        return where;

        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }

    public WhereModel TranslateExpression(Expression exp)
    {
        if (exp is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
            where.SqlOperator = GetSqlOperator(binaryExpression.NodeType);

            if (where.Entity is null)
            {
                var unaryExpression = (UnaryExpression)binaryExpression.Left;

                // Extraindo o valor da expressão subjacente (processo.Id)
                var memberExpression = (MemberExpression)unaryExpression.Operand;
                GetLeftExpression(memberExpression);
            }

            GetValueRightExpression(binaryExpression.Right);
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

            if (memberExpression.Expression is MemberExpression memberExpression2)
            {
                where.MainEntity = memberExpression2.Member.ReflectedType.Name;
            }

        }
        else if (expression is BinaryExpression binaryExpression)
        {
            var left = (MemberExpression)binaryExpression.Left;

            where.Entity = left.Expression.Type.Name;
            where.EntityProp = left.Member.Name;

            if (left.Expression is MemberExpression memberExpression2)
            {
                where.MainEntity = memberExpression2.Member.ReflectedType.Name;
            }
        }
    }

    private WhereModel GetValueRightExpression(Expression rightExpression)
    {

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
        else if (rightExpression is ConstantExpression constantExpression)
        {
            if (constantExpression.Value is null)
            {
                where.SqlOperator = "IS";
                where.Value = "NULL";
            }
            else
            {
                if (HasNullableType(rightExpression))
                {
                    var capturedValue = GetValueNullable(rightExpression);
                    setValueByType(capturedValue);
                }
                else
                {
                    var capturedValue = GetRightExpression(rightExpression);
                    if (capturedValue is null)
                        capturedValue = GetCapturedVariableValue(rightExpression);

                    setValueByType(capturedValue);
                }

            }
        }
        else
        {
            if (HasNullableType(rightExpression))
            {
                var capturedValue = GetValueNullable(rightExpression);
                setValueByType(capturedValue);
            }
            else
            {
                var capturedValue = GetRightExpression(rightExpression);
                if (capturedValue is null)
                    capturedValue = GetCapturedVariableValue(rightExpression);

                setValueByType(capturedValue);
            }

        }

        return where;
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

    private  void ExploreExpression(Expression expression)
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
                ExploreExpression(memberExpression.Expression); // Explora a expressão interna
                break;

            case ConstantExpression constantExpression:
                Console.WriteLine($"ConstantExpression encontrada com valor: {constantExpression.Value}");
                var closureExpression = constantExpression;

                // A classe gerada pelo compilador (<>c__DisplayClass) contém o valor da variável capturada
                var closureObject = closureExpression.Value;
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
            if (!String.IsNullOrEmpty(item))
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
        }

        where.Value = sb.ToString();
        where.SqlOperator = "LIKE";
    }

    private void SetWhereValue(bool value)
    {
        if (value)
        {
            where.Value = "1";
        }
        else
        {
            where.Value = "0";
        }
    }

    private void SetWhereValue(DateTime value)
    {
        var dateString = value.ToString("yyyyMMdd"); ;
        where.Value = $" '{dateString}' ";
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
            ExpressionType.OrElse => "OR",
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
            where.Value = valueObj.ToString();
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
                    else {
                        GetCapturedVariableValueFromFilter((MemberExpression)unaryExpression.Operand);
                    }
                }
            }
        
        return null;
    }

}



