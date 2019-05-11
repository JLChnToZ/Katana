using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public delegate object BuiltInFunction(Runner runner, Node node);

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

        public static void InstallBuiltin(this Runner runner) {
            runner.GetField("+").Value = new BuiltInFunction(BuiltinOperators.Add);
            runner.GetField("-").Value = new BuiltInFunction(BuiltinOperators.Substract);
            runner.GetField("*").Value = new BuiltInFunction(BuiltinOperators.Multiply);
            runner.GetField("/").Value = new BuiltInFunction(BuiltinOperators.Divide);
            runner.GetField("%").Value = new BuiltInFunction(BuiltinOperators.Modulus);
            runner.GetField("&").Value = new BuiltInFunction(BuiltinOperators.And);
            runner.GetField("|").Value = new BuiltInFunction(BuiltinOperators.Or);
            runner.GetField("~").Value = new BuiltInFunction(BuiltinOperators.Not);
            runner.GetField("^").Value = new BuiltInFunction(BuiltinOperators.Xor);
            runner.GetField("<<").Value = new BuiltInFunction(BuiltinOperators.ShiftLeft);
            runner.GetField(">>").Value = new BuiltInFunction(BuiltinOperators.ShiftRight);

            runner.GetField("==").Value = new BuiltInFunction(BuiltinOperators.Equals);
            runner.GetField("!=").Value = new BuiltInFunction(BuiltinOperators.InEqauls);
            runner.GetField(">").Value = new BuiltInFunction(BuiltinOperators.GreaterThen);
            runner.GetField(">=").Value = new BuiltInFunction(BuiltinOperators.GreaterThenEquals);
            runner.GetField("<").Value = new BuiltInFunction(BuiltinOperators.LesserThen);
            runner.GetField("<=").Value = new BuiltInFunction(BuiltinOperators.LesserThenEquals);

            runner.GetField("@").Value = new BuiltInFunction(BuiltinOperators.GetValue);
            runner.GetField("=").Value = new BuiltInFunction(BuiltinOperators.SetValue);
            runner.GetField("$").Value = new BuiltInFunction(BuiltinOperators.RunSequence);
            runner.GetField("&&").Value = new BuiltInFunction(BuiltinOperators.AndThen);
            runner.GetField("||").Value = new BuiltInFunction(BuiltinOperators.OrElse);

            runner.GetField("?").Value = new BuiltInFunction(BuiltinOperators.If);
            runner.GetField("while").Value = new BuiltInFunction(BuiltinOperators.While);
            runner.GetField("function").Value = new BuiltInFunction(BuiltinOperators.Function);
            // runner.GetField("#").Value =
        }
    }
}
