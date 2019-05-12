using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    internal interface IFunction {
        SFieldState Invoke(Runner runner, Node node);
    }

    public static class RunnerHelper {
        public const long MAX_SAFE_INTEGER = (1L << 53) - 1;

        public static bool IsTruly(object obj) {
            switch(Convert.GetTypeCode(obj)) {
                case TypeCode.String:
                    return !string.IsNullOrEmpty(Convert.ToString(obj));
                case TypeCode.Object:
                case TypeCode.DateTime:
                    return true;
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                default:
                    var compare = Convert.ToDouble(obj);
                    return compare != 0 && !double.IsNaN(compare);
            }
        }

        public static FieldType GetFieldType(object value) {
            switch(Convert.GetTypeCode(value)) {
                case TypeCode.String:
                    return FieldType.String;
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                case TypeCode.Empty:
                    return FieldType.Unassigned;
                case TypeCode.Object:
                    if(value is Dictionary<string, FieldState>)
                        return FieldType.Object;
                    if(value is ScriptFunction)
                        return FieldType.Function;
                    if(value is BuiltInFunction)
                        return FieldType.BuiltInFunction;
                    goto case TypeCode.Empty;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return FieldType.Float;
                default:
                    return FieldType.Integer;
            }
        }

        public static void InstallBuiltin(this Runner runner) {
            foreach(var kv in BuiltinOperators.index)
                runner[kv.Key] = kv.Value;
        }
    }
}
