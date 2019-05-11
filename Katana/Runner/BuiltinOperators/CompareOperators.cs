using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {
        public static object Equals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            FieldType fieldType = FieldType.Unassigned;
            string compareStr = null;
            double compareNum = double.NaN;
            object compareObj = null;
            foreach(var child in block) {
                var v = runner.Eval(child);
                switch(fieldType) {
                    case FieldType.Unassigned:
                        if(v is string) {
                            fieldType = FieldType.String;
                            compareStr = Convert.ToString(v);
                            break;
                        }
                        if(v is double || v is long) {
                            fieldType = FieldType.Float;
                            compareNum = Convert.ToDouble(v);
                            break;
                        }
                        fieldType = FieldType.Object;
                        compareObj = v;
                        break;
                    case FieldType.String:
                        if(Convert.ToString(v) != compareStr)
                            return false;
                        break;
                    case FieldType.Float:
                        if(Convert.ToDouble(v) != compareNum)
                            return false;
                        break;
                    case FieldType.Object:
                        if(v != compareObj)
                            return false;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            switch(fieldType) {
                case FieldType.Float:
                case FieldType.String:
                case FieldType.Object:
                    return true;
                default:
                    throw new ArgumentException();
            }
        }

        public static object InEqauls(Runner runner, Node block) =>
            !(bool)Equals(runner, block);

        public static object GreaterThen(Runner runner, Node block) =>
            !(bool)LesserThenEquals(runner, block);

        public static object LesserThen(Runner runner, Node block) =>
            !(bool)GreaterThenEquals(runner, block);

        public static object GreaterThenEquals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.PositiveInfinity;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child));
                if(compareNum < v)
                    return false;
                compareNum = v;
            }
            return true;
        }

        public static object LesserThenEquals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.NegativeInfinity;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child));
                if(compareNum > v)
                    return false;
                compareNum = v;
            }
            return true;
        }
    }
}
