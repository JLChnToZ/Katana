using System;
using System.Collections.Generic;
using System.Text;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {
        internal static readonly Dictionary<string, BuiltInFunction> index =
            new Dictionary<string, BuiltInFunction> {
                ["+"] = new BuiltInFunction(Add, true),
                ["-"] = new BuiltInFunction(Substract, true),
                ["*"] = new BuiltInFunction(Multiply, true),
                ["/"] = new BuiltInFunction(Divide, true),
                ["%"] = new BuiltInFunction(Modulus, true),
                ["&"] = new BuiltInFunction(And, true),
                ["|"] = new BuiltInFunction(Or, true),
                ["~"] = new BuiltInFunction(Not, true),
                ["^"] = new BuiltInFunction(Xor, true),
                ["<<"] = new BuiltInFunction(ShiftLeft, true),
                [">>"] = new BuiltInFunction(ShiftRight, true),

                ["floor"] = new BuiltInFunction(Floor, true),
                ["round"] = new BuiltInFunction(Round, true),
                ["ceil"] = new BuiltInFunction(Ceil, true),
                ["truncate"] = new BuiltInFunction(Truncate, true),
                ["min"] = new BuiltInFunction(Min, true),
                ["max"] = new BuiltInFunction(Max, true),
                ["abs"] = new BuiltInFunction(Abs, true),
                ["sign"] = new BuiltInFunction(Sign, true),
                ["exp"] = new BuiltInFunction(Exp, true),
                ["sqrt"] = new BuiltInFunction(Sqrt, true),
                ["pow"] = new BuiltInFunction(Pow, true),
                ["log"] = new BuiltInFunction(Log, true),
                ["log10"] = new BuiltInFunction(Log10, true),
                ["sin"] = new BuiltInFunction(Sin, true),
                ["cos"] = new BuiltInFunction(Cos, true),
                ["tan"] = new BuiltInFunction(Tan, true),
                ["asin"] = new BuiltInFunction(Asin, true),
                ["acos"] = new BuiltInFunction(Acos, true),
                ["atan"] = new BuiltInFunction(Atan, true),
                ["sinh"] = new BuiltInFunction(Sinh, true),
                ["cosh"] = new BuiltInFunction(Cosh, true),
                ["tanh"] = new BuiltInFunction(Tanh, true),

                ["=="] = new BuiltInFunction(Equals, true),
                ["!="] = new BuiltInFunction(InEqauls, true),
                [">"] = new BuiltInFunction(GreaterThen, true),
                [">="] = new BuiltInFunction(GreaterThenEquals, true),
                ["<"] = new BuiltInFunction(LesserThen, true),
                ["<="] = new BuiltInFunction(LesserThenEquals, true),
                ["@"] = new BuiltInFunction(GetValue, true),
                ["="] = new BuiltInFunction(SetValue, true),

                ["fromchar"] = new BuiltInFunction(FromChar, true),
                ["fromchars"] = new BuiltInFunction(FromCharArray, true),
                ["tochar"] = new BuiltInFunction(ToChar, true),
                ["tochars"] = new BuiltInFunction(ToCharArray, true),

                ["$"] = new BuiltInFunction(RunSequence, false),
                ["&&"] = new BuiltInFunction(AndThen, false),
                ["||"] = new BuiltInFunction(OrElse, false),
                ["?"] = new BuiltInFunction(If, false),
                ["while"] = new BuiltInFunction(While, false),
                ["function"] = new BuiltInFunction(Function, false),
                ["return"] = new BuiltInFunction(Return, true),
                ["typeof"] = new BuiltInFunction(TypeOf, true),

                [""] = new BuiltInFunction(CreateArray, true),
                ["slice"] = new BuiltInFunction(Slice, true),
                ["length"] = new BuiltInFunction(Length, true),
                ["any"] = new BuiltInFunction(Any, true),

                // ["#"] = ...,
            };
    }
}
