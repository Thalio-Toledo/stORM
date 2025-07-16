using System.Reflection;
using System.Text.RegularExpressions;

namespace stORM.utils
{
    public class UtilsService
    {
        public static string GenerateAlias(string entity)
        {
            string pattern = @"([A-Z][a-z]{0,2})";
            var partialAliases = Regex.Matches(entity, pattern);
            string alias = string.Join("", partialAliases);

            return $"[TB_{alias}]";
        }

        public static DateTime AddDiasUteis(DateTime date, int dias)
        {
            int i = 0;

            while (i < dias)
            {
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        date = date.AddDays(2);
                        i++;
                        break;
                    case DayOfWeek.Friday:
                        date = date.AddDays(3);
                        i++;
                        break;
                    default:
                        date = date.AddDays(1);
                        i++;
                        break;
                }
            }

            return date;
        }

        public static DateTime AddHoras(DateTime date, int horas)
        {
            return date.AddHours(horas);
        }

        public static DateTime AddHorasUteis(DateTime date, int horas)
        {
            int i = 0;

            while (i <= horas)
            {
                date = date.AddHours(1);
                if (date.Hour >= 8 && date.Hour < 18)
                {
                    i++;
                }

            }

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        public static DateTime AddTimestamp(DateTime date, long timestamp)
        {
            long dias = timestamp / 24 / 60 / 60 / 10000;
            if (dias > 0)
            {
                date = AddDiasUteis(date, 1);
            }

            return date.AddMilliseconds(timestamp);
        }

        public static bool IsDateNotNull(DateTime date)
        {
            return date.Year >= 2000;
        }

        public static bool IsDateNotNull(DateTime? date)
        {
            if (date == null)
            {
                return false;
            }

            return IsDateNotNull(ToDate(date));
        }

        public static bool IsDateNull(DateTime date)
        {
            return date.Year < 2000;
        }

        public static string RandomString(int length)
        {
            Random random = new();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsNotNull(long? content)
        {
            if (content == null)
            {
                return false;
            }

            if (content == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(Enum? content)
        {
            if (content == null)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(Guid? content)
        {
            if (content == Guid.Empty)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(decimal? content)
        {
            if (content.HasValue)
            {
                return false;
            }
            if (content == null)
            {
                return false;
            }


            return true;
        }
        public static bool IsNotNull(double? content)
        {
            if (content.HasValue)
            {
                return false;
            }
            if (content == null)
            {
                return false;
            }

            if (content == 0.0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(List<string> content)
        {
            if (content == null)
            {
                return false;
            }

            if (content.Count == 0)
            {
                return false;
            }

            return true;
        }
        public static bool IsNotNull(List<object> content)
        {
            if (content == null)
            {
                return false;
            }

            if (content.Count == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(List<long> content)
        {
            if (content == null)
            {
                return false;
            }

            if (content.Count == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(bool? content)
        {
            if (content == null)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(string content)
        {
            if (content == null)
            {
                return false;
            }

            if (content.Trim().Length == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotNull(DateTime content)
        {
            if (content == DateTime.MinValue)
            {
                return false;
            }

            return true;
        }

        //public static bool IsNotNull(List<dynamic> content)
        //{
        //    if (content == null)
        //    {
        //        return false;
        //    }

        //    if (content.Count == 0)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public static bool IsNull(long? content)
        {
            return !IsNotNull(content);
        }

        public static bool IsNull(double? content)
        {
            return !IsNotNull(content);
        }

        public static bool IsNull(bool? content)
        {
            return !IsNotNull(content);
        }

        public static bool IsNull(string content)
        {
            return !IsNotNull(content);
        }

        public static bool IsNull(DateTime content)
        {
            return !IsNotNull(content);
        }

        public static bool IsNull(List<dynamic> content)
        {
            return !IsNotNull(content);
        }


        public static string RemoverNumerosEspeciais(string input)
        {
            string pattern = "[^a-zA-Z\\s\t]";
            string resultado = Regex.Replace(input, pattern, "");

            resultado = resultado.Replace("\t", "");

            return resultado;
        }
        public static DateTime ToDate(DateTime? content)
        {
            if (content == null)
            {
                return DateTime.MinValue;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public static string NormalizeEmptyStringForSqlScript(string content)
        {
            return IsNotNull(content) ? content : "''";
        }

        public static string toSQLDate(DateTime data)
        {
            try
            {
                return $"'{data.ToString("yyyyMMdd")}'";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "00010101";
            }
        }

        public static string SetColumnSQLType(Guid column) => " UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID() ";
        public static string SetColumnSQLType(string column) => " NVARCHAR(MAX) NOT NULL";
        public static string SetColumnSQLType(int column) => " INT NOT NULL";
        public static string SetColumnSQLType(DateTime column) => " DATETIME NOT NULL ";
        public static string SetColumnSQLType(bool column) => " BIT NOT NULL DEFAULT 1 ";

        public static bool IsNullableProperty(PropertyInfo property)
        {
            // 1. Verificar se é Nullable<T>
            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                return true;

            // 2. Verificar se é um Nullable Reference Type (C# 8+)
            var nullableAttribute = property.CustomAttributes
                .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

            return nullableAttribute != null;
        }
    }
    public static class StringExtensions
    {
        public static string NormalizeQuery(this string input) =>
            string.Concat(input
                .Where(c => !char.IsWhiteSpace(c))) // Remove espaços, tabs e quebras de linha
                .Replace("\"", "")                  // Remove aspas duplas extras
                .Trim();
    }

    

}