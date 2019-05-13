using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {
        public static Field Equals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var compareFieldType = FieldType.Unassigned;
            string compareStr = null;
            double compareNum = double.NaN;
            object compareObj = null;
            foreach(var child in block) {
                var v = runner.Eval(child);
                switch(compareFieldType) {
                    case FieldType.Unassigned:
                        switch(v.FieldType) {
                            case FieldType.String:
                                compareFieldType = FieldType.String;
                                compareStr = v.StringValue;
                                break;
                            case FieldType.Integer:
                            case FieldType.Float:
                                compareFieldType = FieldType.Float;
                                compareNum = v.FloatValue;
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
                        if(v.Value != compareObj)
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

        public static Field InEqauls(Runner runner, Node block) =>
            1 - (int)Equals(runner, block);

        public static Field GreaterThen(Runner runner, Node block) =>
            1 - (int)LesserThenEquals(runner, block);

        public static Field LesserThen(Runner runner, Node block) =>
            1 - (int)GreaterThenEquals(runner, block);

        public static Field GreaterThenEquals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.PositiveInfinity;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(compareNum < v)
                    return 0;
                compareNum = v;
            }
            return 1;
        }

        public static Field LesserThenEquals(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            double compareNum = double.NegativeInfinity;
            foreach(var child in block) {
                var v = runner.Eval(child).FloatValue;
                if(compareNum > v)
                    return 0;
                compareNum = v;
            }
            return 1;
        }
    }
}
