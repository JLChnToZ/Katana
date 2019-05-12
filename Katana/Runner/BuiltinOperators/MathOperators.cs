using System;
using System.Text;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        #region Basic
        public static object Add(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            fieldType = FieldType.Unassigned;
            StringBuilder resultStr = null;
            double? resultNum = null;
            List<FieldState> resultList = null;
            foreach(var child in block) {
                var v = runner.Eval(child, out var ft);
                switch(fieldType) {
                    case FieldType.Unassigned:
                        switch(ft) {
                            case FieldType.String:
                                fieldType = ft;
                                resultStr = new StringBuilder(Convert.ToString(v));
                                break;
                            case FieldType.Integer:
                            case FieldType.Float:
                                fieldType = FieldType.Float;
                                resultNum = Convert.ToDouble(v);
                                break;
                            case FieldType.Array:
                                fieldType = FieldType.Array;
                                resultList = v as List<FieldState>;
                                break;
                        }
                        goto default;
                    case FieldType.Array:
                        if(ft == FieldType.Array)
                            foreach(var entry in v as List<FieldState>)
                                resultList.Add(new FieldState(entry.State));
                        else
                            resultList.Add(new FieldState(new SFieldState {
                                value = v,
                                fieldType = ft,
                            }));
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

        public static object Substract(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(result.HasValue)
                    result += v;
                else
                    result = -v;
            }
            if(block.Count > 1)
                result = -result;
            fieldType = FieldType.Float;
            return result.GetValueOrDefault(double.NaN);
        }

        public static object Multiply(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(result.HasValue)
                    result *= v;
                else
                    result = v;
            }
            fieldType = FieldType.Float;
            return result.GetValueOrDefault(double.NaN);
        }

        public static object Divide(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(result.HasValue)
                    result /= v;
                else
                    result = v;
            }
            fieldType = FieldType.Float;
            return result.GetValueOrDefault(double.NaN);
        }

        public static object Modulus(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(result.HasValue)
                    result %= v;
                else
                    result = v;
            }
            fieldType = FieldType.Float;
            return result.GetValueOrDefault(double.NaN);
        }

        public static object Pow(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double? result = null;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(result.HasValue)
                    result = Math.Pow(result.Value, v);
                else
                    result = v;
            }
            fieldType = FieldType.Float;
            return result;
        }
        #endregion

        #region Advanced
        public static object Min(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double result = double.PositiveInfinity;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                result = Math.Min(result, v);
            }
            fieldType = FieldType.Float;
            return result;
        }

        public static object Max(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double result = double.NegativeInfinity;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                result = Math.Max(result, v);
            }
            fieldType = FieldType.Float;
            return result;
        }

        public static object Abs(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Abs(a);
        }

        public static object Sign(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Sign(a);
        }

        public static object Floor(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Floor(a);
        }

        public static object Round(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Round(a);
        }

        public static object Ceil(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Ceiling(a);
        }

        public static object Truncate(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Truncate(a);
        }

        public static object Exp(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Exp(a);
        }

        public static object Sqrt(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Sqrt(a);
        }

        public static object Log(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1 || block.Count > 2)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            if(block.Count == 2) {
                var b = Convert.ToDouble(runner.Eval(block[1], out _));
                return Math.Log(a, b);
            }
            return Math.Log(a);
        }

        public static object Log10(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Log10(a);
        }

        public static object Sin(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Sin(a);
        }

        public static object Cos(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Cos(a);
        }

        public static object Tan(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Tan(a);
        }

        public static object Asin(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Asin(a);
        }

        public static object Acos(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Acos(a);
        }

        public static object Atan(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1 || block.Count > 2)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            if(block.Count == 2) {
                var b = Convert.ToDouble(runner.Eval(block[1], out _));
                return Math.Atan2(a, b);
            }
            return Math.Atan(a);
        }

        public static object Sinh(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Sinh(a);
        }

        public static object Cosh(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Cosh(a);
        }

        public static object Tanh(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToDouble(runner.Eval(block[0], out _));
            fieldType = FieldType.Float;
            return Math.Tanh(a);
        }
        #endregion

        #region Bitwise
        public static object And(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = Convert.ToInt64(runner.Eval(child, out _));
                if(result.HasValue)
                    result &= v;
                else
                    result = v;
            }
            fieldType = FieldType.Integer;
            return result.GetValueOrDefault(0);
        }

        public static object Or(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = Convert.ToInt64(runner.Eval(child, out _));
                if(result.HasValue)
                    result |= v;
                else
                    result = v;
            }
            fieldType = FieldType.Integer;
            return result.GetValueOrDefault(0);
        }

        public static object Xor(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            long? result = null;
            foreach(var child in block) {
                var v = Convert.ToInt64(runner.Eval(child, out _));
                if(result.HasValue)
                    result ^= v;
                else
                    result = v;
            }
            fieldType = FieldType.Integer;
            return result.GetValueOrDefault(0);
        }

        public static object Not(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var a = Convert.ToInt64(runner.Eval(block[0], out _));
            fieldType = FieldType.Integer;
            return (~a) & RunnerHelper.MAX_SAFE_INTEGER;
        }

        public static object ShiftLeft(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 2)
                throw new ArgumentException();
            var a = Convert.ToInt64(runner.Eval(block[0], out _));
            var b = Convert.ToInt32(runner.Eval(block[1], out _));
            fieldType = FieldType.Integer;
            return (a << b) & RunnerHelper.MAX_SAFE_INTEGER;
        }

        public static object ShiftRight(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 2)
                throw new ArgumentException();
            var a = Convert.ToInt64(runner.Eval(block[0], out _));
            var b = Convert.ToInt32(runner.Eval(block[1], out _));
            fieldType = FieldType.Integer;
            return (a >> b) & RunnerHelper.MAX_SAFE_INTEGER;
        }
        #endregion
    }
}
