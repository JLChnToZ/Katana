using System;
using System.Collections;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static Field GetValue(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            var tag = Convert.ToString(block[0].Tag);
            Field field = runner.GetFieldOrInit(tag, block.Count > 1 ? FieldType.Object : FieldType.Unassigned);
            for(int i = 1, l = block.Count - 1; i < l; i++)
                field = field.GetAndEnsureType(runner.Eval(block[i]), FieldType.Object);
            return field;
        }

        public static Field SetValue(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var tag = Convert.ToString(block[0].Tag);
            if(block.Count == 2)
                return runner.SetField(tag, runner.Eval(block[1]));
            Field field = runner.GetFieldOrInit(tag, FieldType.Object);
            for(int i = 1, l = block.Count - 2; i < l; i++)
                field = field.GetAndEnsureType(runner.Eval(block[i]), FieldType.Object);
            return field[runner.Eval(block[block.Count - 2])] =
                runner.Eval(block[block.Count - 1]);
        }

        public static Field Length(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            var obj = runner.Eval(block[1]);
            switch(obj.FieldType) {
                case FieldType.String:
                    return obj.StringValue.Length;
                case FieldType.Array:
                case FieldType.Object:
                    return obj.Count;
                default:
                    return 0;
            }
        }

        public static Field TypeOf(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            switch(runner.Eval(block[1]).FieldType) {
                case FieldType.Unassigned: return "undefined";
                case FieldType.Integer: return "integer";
                case FieldType.Float: return "float";
                case FieldType.String: return "string";
                case FieldType.Object: return "object";
                case FieldType.Array: return "array";
                case FieldType.Function:
                case FieldType.BuiltInFunction: return "function";
                default: return "unknown";
            }
        }

        public static Field RunSequence(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            Field result = default;
            foreach(var child in block) {
                result = runner.Eval(child);
                var field = runner.GetFieldOrInit(Convert.ToString(child));
                if(field.FieldType == FieldType.BuiltInFunction &&
                    field.Value == index["return"])
                    return result;
            }
            return result;
        }

        public static Field AndThen(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            Field result = default;
            foreach(var child in block) {
                var nextResult = runner.Eval(child);
                if(!nextResult.IsTruly)
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static Field OrElse(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            Field result = default;
            foreach(var child in block) {
                var nextResult = runner.Eval(child);
                if(nextResult.IsTruly)
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static Field Return(Runner runner, Node block) {
            if(block.Count > 1)
                throw new ArgumentException();
            return block.Count == 1 ? runner.Eval(block[0]) : default;
        }

        public static Field If(Runner runner, Node block) {
            if(block.Count < 2 || block.Count > 3)
                throw new ArgumentException();
            if(runner.Eval(block[0]).IsTruly)
                return runner.Eval(block[1]);
            else if(block.Count == 3)
                return runner.Eval(block[2]);
            return default;
        }

        public static Field While(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            Field result = default;
            while(runner.Eval(block[0]).IsTruly)
                result = runner.Eval(block[1]);
            return result;
        }

        public static Field Function(Runner runner, Node block) {
            if(block.Count < 3)
                throw new ArgumentException();
            var tag = Convert.ToString(block[0].Tag);
            var result = new ScriptFunction(
                block[block.Count - 2],
                block[block.Count - 1]);
            if(block.Count == 3)
                return runner.SetField(tag, result);
            Field field = runner.GetFieldOrInit(tag, FieldType.Object);
            for(int i = 1, l = block.Count - 3; i < l; i++)
                field = field.GetAndEnsureType(runner.Eval(block[i]), FieldType.Object);
            return field[runner.Eval(block[block.Count - 3])] = result;
        }

        public static Field Try(Runner runner, Node block) {
            if(block.Count != 1 && block.Count != 3)
                throw new ArgumentException();
            try {
                return runner.Eval(block[0]);
            } catch(Exception ex) {
                var errArg = (string)runner.Eval(block[1]);
                try {
                    runner.PushContext();
                    runner.SetField(errArg, ex.Message, true);
                    Field errResult = default;
                    if(block.Count == 3)
                        errResult = runner.Eval(block[2]);
                    return errResult;
                } finally {
                    runner.PopContext();
                }
            }
        }
    }
}
