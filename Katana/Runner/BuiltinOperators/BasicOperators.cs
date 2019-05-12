using System;
using System.Collections;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static object GetValue(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            FieldState field = null;
            foreach(var child in block) {
                var v = runner.Eval(child, out var ft);
                field = field != null ?
                    ft == FieldType.Float || ft == FieldType.Integer ?
                        field[Convert.ToInt32(v)] :
                        field[Convert.ToString(v)] :
                    runner.GetField(Convert.ToString(v));
            }
            fieldType = field.FieldType;
            return field.Value;
        }

        public static object SetValue(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            FieldState field = null;
            var lastChild = block[block.Count - 1];
            foreach(var child in block) {
                var v = runner.Eval(child, out var ft);
                if(child == lastChild)
                    field.Value = v;
                else
                    field = field != null ?
                        ft == FieldType.Float || ft == FieldType.Integer ?
                            field[Convert.ToInt32(v)] :
                            field[Convert.ToString(v)] :
                        runner.GetField(Convert.ToString(v));
            }
            fieldType = field.FieldType;
            return field.Value;
        }

        public static object Length(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            var obj = runner.Eval(block[1], out var ft);
            fieldType = FieldType.Integer;
            switch(ft) {
                case FieldType.String:
                    return (obj as string).Length;
                case FieldType.Array:
                case FieldType.Object:
                    return (obj as ICollection).Count;
                default:
                    return 0;
            }
        }

        public static object TypeOf(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            runner.Eval(block[1], out var ft);
            fieldType = FieldType.String;
            switch(ft) {
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

        public static object RunSequence(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            fieldType = FieldType.Unassigned;
            foreach(var child in block) {
                result = runner.Eval(child, out fieldType);
                var field = runner.GetField(child.Tag as string);
                if(field.FieldType == FieldType.BuiltInFunction &&
                    field.Value == index["return"])
                    return result;
            }
            return result;
        }

        public static object AndThen(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            fieldType = FieldType.Unassigned;
            foreach(var child in block) {
                var nextResult = runner.Eval(child, out fieldType);
                if(!RunnerHelper.IsTruly(nextResult))
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static object OrElse(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            fieldType = FieldType.Unassigned;
            foreach(var child in block) {
                var nextResult = runner.Eval(child, out fieldType);
                if(RunnerHelper.IsTruly(nextResult))
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static object Return(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count > 1)
                throw new ArgumentException();
            fieldType = FieldType.Unassigned;
            return block.Count == 1 ? runner.Eval(block[0], out fieldType) : null;
        }

        public static object If(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2 || block.Count > 3)
                throw new ArgumentException();
            if(RunnerHelper.IsTruly(runner.Eval(block[0], out _)))
                return runner.Eval(block[1], out fieldType);
            else if(block.Count == 3)
                return runner.Eval(block[2], out fieldType);
            fieldType = FieldType.Unassigned;
            return null;
        }

        public static object While(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            object result = null;
            fieldType = FieldType.Unassigned;
            while(RunnerHelper.IsTruly(runner.Eval(block[0], out _)))
                result = runner.Eval(block[1], out fieldType);
            return result;
        }

        public static object Function(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 3)
                throw new ArgumentException();
            FieldState field = null;
            var args = block[block.Count - 2];
            var body = block[block.Count - 1];
            for(int i = 0, l = block.Count - 2; i < l; i++) {
                var v = runner.Eval(block[i], out var ft);
                field = field != null ?
                    ft == FieldType.Float || ft == FieldType.Integer ?
                        field[Convert.ToInt32(v)] :
                        field[Convert.ToString(v)] :
                    runner.GetField(Convert.ToString(v));
            }
            field.Value = new ScriptFunction(args, body);
            fieldType = field.FieldType;
            return field.Value;
        }
    }
}
