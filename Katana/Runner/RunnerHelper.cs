using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    internal interface IFunction {
        Field Invoke(Runner runner, Node node);
    }

    public static class RunnerHelper {
        public const long MAX_SAFE_INTEGER = (1L << 53) - 1;

        public static FieldType GetFieldType(object value) {
            switch(Convert.GetTypeCode(value)) {
                case TypeCode.String:
                    return FieldType.String;
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                case TypeCode.Empty:
                    return FieldType.Unassigned;
                case TypeCode.Object:
                    if(value is Dictionary<string, Field>)
                        return FieldType.Object;
                    if(value is List<Field>)
                        return FieldType.Array;
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
