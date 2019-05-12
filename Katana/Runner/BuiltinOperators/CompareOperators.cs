using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {
        public static object Equals(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            var compareFieldType = FieldType.Unassigned;
            string compareStr = null;
            double compareNum = double.NaN;
            object compareObj = null;
            fieldType = FieldType.Integer;
            foreach(var child in block) {
                var v = runner.Eval(child, out var ft);
                switch(compareFieldType) {
                    case FieldType.Unassigned:
                        switch(ft) {
                            case FieldType.String:
                                compareFieldType = ft;
                                compareStr = Convert.ToString(v);
                                break;
                            case FieldType.Integer:
                            case FieldType.Float:
                                compareFieldType = FieldType.Float;
                                compareNum = Convert.ToDouble(v);
                                break;
                        }
                        compareFieldType = FieldType.Object;
                        compareObj = v;
                        break;
                    case FieldType.String:
                        if(Convert.ToString(v) != compareStr)
                            return 0;
                        break;
                    case FieldType.Float:
                        if(Convert.ToDouble(v) != compareNum)
                            return 0;
                        break;
                    case FieldType.Object:
                        if(v != compareObj)
                            return 0;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            switch(compareFieldType) {
                case FieldType.Float:
                case FieldType.String:
                case FieldType.Object:
                    return 1;
                default:
                    throw new ArgumentException();
            }
        }

        public static object InEqauls(Runner runner, Node block, out FieldType fieldType) =>
            1 - (int)Equals(runner, block, out fieldType);

        public static object GreaterThen(Runner runner, Node block, out FieldType fieldType) =>
            1 - (int)LesserThenEquals(runner, block, out fieldType);

        public static object LesserThen(Runner runner, Node block, out FieldType fieldType) =>
            1 - (int)GreaterThenEquals(runner, block, out fieldType);

        public static object GreaterThenEquals(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.PositiveInfinity;
            fieldType = FieldType.Integer;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(compareNum < v)
                    return 0;
                compareNum = v;
            }
            return 1;
        }

        public static object LesserThenEquals(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.NegativeInfinity;
            fieldType = FieldType.Integer;
            foreach(var child in block) {
                var v = Convert.ToDouble(runner.Eval(child, out _));
                if(compareNum > v)
                    return 0;
                compareNum = v;
            }
            return 1;
        }
    }
}
