using System;
using System.Text;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        #region Basic
        public static Field Add(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var fieldType = FieldType.Unassigned;
            StringBuilder resultStr = null;
            double? resultNum = null;
            Field resultList = default;
            foreach(var child in block) {
                var v = runner.Eval(child);
                switch(fieldType) {
                    case FieldType.Unassigned:
                        switch(v.FieldType) {
                            case FieldType.String:
                                fieldType = FieldType.String;
                                resultStr = new StringBuilder((string)v);
                                break;
                            case FieldType.Integer:
                            case FieldType.Float:
                                fieldType = FieldType.Float;
                                resultNum = Convert.ToDouble(v);
                                break;
                            case FieldType.Array:
                                fieldType = FieldType.Array;
                                resultList = v;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case FieldType.Array:
                        if(v.FieldType == FieldType.Array)
                            foreach(var entry in v)
                                resultList.Add(entry);
                        else
                            resultList.Add(v);
                        break;
                    case FieldType.String:
                        resultStr.Append(v);
                        break;
                    case FieldType.Float:
                        resultNum += Convert.ToDouble(v);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            switch(fieldType) {
                case FieldType.Float:
                    return resultNum.GetValueOrDefault(double.NaN);
                case FieldType.String:
                    return resultStr.ToString();
                case FieldType.Array:
                    return resultList;
                default:
                    throw new ArgumentException();
            }
        }

        public static Field Substract(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(result.HasValue)
                    result += v;
                else
                    result = -v;
            }
            if(block.Count > 1)
                result = -result;
            return result.GetValueOrDefault(double.NaN);
        }

        public static Field Multiply(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(result.HasValue)
                    result *= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(double.NaN);
        }

        public static Field Divide(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(result.HasValue)
                    result /= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(double.NaN);
        }

        public static Field Modulus(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(result.HasValue)
                    result %= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(double.NaN);
        }

        public static Field Pow(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(result.HasValue)
                    result = Math.Pow(result.Value, v);
                else
                    result = v;
            }
            return result.GetValueOrDefault(double.NaN);
        }
        #endregion

        #region Advanced
        public static Field Min(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double result = double.PositiveInfinity;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                result = Math.Min(result, v);
            }
            return result;
        }

        public static Field Max(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double result = double.NegativeInfinity;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                result = Math.Max(result, v);
            }
            return result;
        }

        public static Field Abs(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Abs(a);
        }

        public static Field Sign(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Sign(a);
        }

        public static Field Floor(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Floor(a);
        }

        public static Field Round(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Round(a);
        }

        public static Field Ceil(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Ceiling(a);
        }

        public static Field Truncate(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Truncate(a);
        }

        public static Field Exp(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Exp(a);
        }

        public static Field Sqrt(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Sqrt(a);
        }

        public static Field Log(Runner runner, Node block) {
            if(block.Count < 1 || block.Count > 2)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            if(block.Count == 2) {
                var b = runner.Eval(block[1]).FloatValue;
                return Math.Log(a, b);
            }
            return Math.Log(a);
        }

        public static Field Log10(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Log10(a);
        }

        public static Field Sin(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Sin(a);
        }

        public static Field Cos(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Cos(a);
        }

        public static Field Tan(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Tan(a);
        }

        public static Field Asin(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Asin(a);
        }

        public static Field Acos(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Acos(a);
        }

        public static Field Atan(Runner runner, Node block) {
            if(block.Count < 1 || block.Count > 2)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            if(block.Count == 2) {
                var b = runner.Eval(block[1]).FloatValue;
                return Math.Atan2(a, b);
            }
            return Math.Atan(a);
        }

        public static Field Sinh(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Sinh(a);
        }

        public static Field Cosh(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Cosh(a);
        }

        public static Field Tanh(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).FloatValue;
            return Math.Tanh(a);
        }
        #endregion

        #region Bitwise
        public static Field And(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).IntValue;
                if(result.HasValue)
                    result &= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(0);
        }

        public static Field Or(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).IntValue;
                if(result.HasValue)
                    result |= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(0);
        }

        public static Field Xor(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = runner.Eval(child).IntValue;
                if(result.HasValue)
                    result ^= v;
                else
                    result = v;
            }
            return result.GetValueOrDefault(0);
        }

        public static Field Not(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).IntValue;
            return (~a) & RunnerHelper.MAX_SAFE_INTEGER;
        }

        public static Field ShiftLeft(Runner runner, Node block) {
            if(block.Count != 2)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).IntValue;
            var b = (int)runner.Eval(block[1]);
            return (a << b) & RunnerHelper.MAX_SAFE_INTEGER;
        }

        public static Field ShiftRight(Runner runner, Node block) {
            if(block.Count != 2)
                throw new ArgumentException();
            var a = runner.Eval(block[0]).IntValue;
            var b = (int)runner.Eval(block[1]);
            return (a >> b) & RunnerHelper.MAX_SAFE_INTEGER;
        }
        #endregion
    }
}
