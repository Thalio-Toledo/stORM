using BonesCoreOrm.Generators.Intefaces;
using stORM.Models;
using stORM.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static stORM.Models.GroupByModel;

namespace stORM.stORM_Core.Generators
{
    public sealed class CreateGen : IGenerator
    {
        public string Generate(dynamic entity)
        {
            var script = new StringBuilder();
            script.Append(" CREATE TABLE ");
            script.AppendLine($" {entity.Name} (");
          

            void GetEntityColums(dynamic entity)
            {
                var primitiveProps = new List<PropertyInfo>();

                foreach (PropertyInfo prop in entity.GetProperties())
                {
                    if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string)) continue;
                    if ( prop.GetCustomAttributes(typeof(KeyAttribute), false).Any()) continue;
                    if ( prop.GetValue(entity, null) != null) continue;
                    else
                        primitiveProps.Add(prop);
                }

                script.AppendLine($"{GetPrimaryKey(entity)} INT IDENTITY(1,1) PRIMARY KEY,");


                primitiveProps.ForEach(prop =>
                {
                    if (prop.PropertyType == typeof(int))
                    {
                        script.AppendLine($"{prop.Name} INT ");
                        if (UtilsService.IsNullableProperty(prop)) script.Append(" NULL ,");
                        else
                            script.Append(" NOT NULL ,");
                    }
                    if (prop.PropertyType == typeof(string))
                    {
                        script.AppendLine($"{prop.Name} NVARCHAR(MAX) ");
                        if (UtilsService.IsNullableProperty(prop)) script.Append(" NULL ,");
                        else
                            script.Append(" NOT NULL ,");
                    }
                    if (prop.PropertyType == typeof(bool))
                    {
                        script.AppendLine($"{prop.Name} BIT ");
                        if (UtilsService.IsNullableProperty(prop)) script.Append("  NULL DEFAULT 1 ,");
                        else
                            script.Append(" NOT NULL DEFAULT 1 ,");
                    }
                    if (prop.PropertyType == typeof(DateTime))
                    {
                        script.AppendLine($"{prop.Name} DATETIME  ");
                        if (UtilsService.IsNullableProperty(prop)) script.Append(" NULL NULL ,");
                        else
                            script.Append(" NOT NULL ,");
                    }
                });
            }
            GetEntityColums(entity);

            script.AppendLine(")");

            return script.ToString();
        }

        private string GetPrimaryKey(Type entity) =>
            entity
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.Name ??
                throw new Exception($"Primary Key from {entity.Name} was not found!");

        private Type GetPrimaryKeyType(Type entity) =>
            entity
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() is not null)?.PropertyType ??
                throw new Exception($"Primary Key from {entity.Name} was not found!");

        private void SetColumnsTypeSQL(List<PropertyInfo> primitiveProps)
        {
            primitiveProps.ForEach(prop =>
            {
                if (prop.PropertyType == typeof(int))
                {
                    var columnScript = $"{prop.Name} INT ";
                    if (UtilsService.IsNullableProperty(prop)) columnScript += " NOT NULL ";
                    else
                        columnScript += " NULL ";
                }
                if (prop.PropertyType == typeof(string))
                {
                    var columnScript = $"{prop.Name} NVARCHAR(MAX) ";
                    if (UtilsService.IsNullableProperty(prop)) columnScript += " NOT NULL ";
                    else
                        columnScript += " NULL ";
                } 
                if (prop.PropertyType == typeof(bool))
                {
                    var columnScript = $"{prop.Name} BIT  ";
                    if (UtilsService.IsNullableProperty(prop)) columnScript += " NOT NULL DEFAULT 1 ";
                    else
                        columnScript += " NULL DEFAULT 1 ";
                } 
                if (prop.PropertyType == typeof(DateTime))
                {
                    var columnScript = $"{prop.Name} DATETIME   ";
                    if (UtilsService.IsNullableProperty(prop)) columnScript += " NOT NULL ";
                    else
                        columnScript += " NULL DEFAULT 1 ";
                }
            });
        }


    }
}
