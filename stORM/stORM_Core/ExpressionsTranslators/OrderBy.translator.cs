using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static stORM.Models.GroupByModel;
using static Dapper.SqlMapper;

namespace BonesCore.BonesCoreOrm.ExpressionsTranslators;

public class OrderByTranslator
{
    public Type _entity;
    public Expression _expression;
    private OrderByModel _orderby = new OrderByModel();
    public OrderByTranslator(Type entity, Expression expression )
    {
        _entity = entity;
        _expression = expression;

        TranslateExpression();
    }

    public OrderByModel TranslateExpression()
    {
        if (_expression is BinaryExpression binaryExpression)
        {
            GetLeftExpression(binaryExpression.Left);
        }


        if (_expression is MemberExpression memberExpression)
        {
            _orderby.Entity = memberExpression.Expression.Type.Name;
            _orderby.EntityProp = memberExpression.Member.Name;
            return _orderby;
        }


        throw new NotSupportedException($"O tipo de expressão '{_expression.GetType()}' não é suportado.");
    }

    private void GetLeftExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
                _orderby.Entity = memberExpression.Expression.Type.Name;
                _orderby.EntityProp = memberExpression.Member.Name;
        }
    }
    public OrderByModel GenerateOrderBy()
    {
        var mainEntityName = _entity.Name;


        if (mainEntityName == _orderby.Entity)
        {
            foreach (PropertyInfo mainTableProp in _entity.GetProperties())
            {
                if (mainTableProp.Name == "TableAlias")
                {
                    var instance = Activator.CreateInstance(_entity);
                    var propValue = mainTableProp.GetValue(instance, null);
                    _orderby.EntityAlias = propValue?.ToString();
                    continue;
                }
            }
        }
        else
        {
            foreach (PropertyInfo mainTableProp in _entity.GetProperties())
            {
                if (mainTableProp.Name == _orderby.Entity)
                {
                    Type nestedEntity = mainTableProp.PropertyType;

                    foreach (PropertyInfo nestedClassProp in nestedEntity.GetProperties())
                    {

                        if (nestedClassProp.Name == "TableAlias")
                        {
                            var instance = Activator.CreateInstance(nestedEntity);
                            var propValue = nestedClassProp.GetValue(instance, null);
                            _orderby.EntityAlias = propValue?.ToString();
                            continue;
                        }

                    }

                }

            }
        }

        return _orderby;


    }
}
