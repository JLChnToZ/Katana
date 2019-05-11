using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {
        private static BuiltInFunction returnFunc = Return;

        public static object GetValue(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            FieldState field = null;
            foreach(var child in block) {
                var v = Convert.ToString(runner.Eval(child));
                field = field != null ?
                    field.GetSubField(runner, v) :
                    runner.GetField(v);
            }
            return field.Value;
        }

        public static object SetValue(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            FieldState field = null;
            var lastChild = block[block.Count - 1];
            foreach(var child in block) {
                var v = Convert.ToString(runner.Eval(child));
                if(child == lastChild)
                    field.Value = v;
                else
                    field = field != null ?
                    field.GetSubField(runner, v) :
                    runner.GetField(v);
            }
            return field.Value;
        }

        public static object RunSequence(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            foreach(var child in block) {
                result = runner.Eval(child);
                var field = runner.GetField(child.Tag as string);
                if(field.fieldType == FieldType.BuiltInFunction &&
                    (field.Value as BuiltInFunction) == returnFunc)
                    return result;
            }
            return result;
        }

        public static object AndThen(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            foreach(var child in block) {
                var nextResult = runner.Eval(child);
                if(!RunnerHelper.IsTruly(nextResult))
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static object OrElse(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            object result = null;
            foreach(var child in block) {
                var nextResult = runner.Eval(child);
                if(RunnerHelper.IsTruly(nextResult))
                    return nextResult;
                result = nextResult;
            }
            return result;
        }

        public static object Return(Runner runner, Node block) {
            if(block.Count > 1)
                throw new ArgumentException();
            return block.Count == 1 ? runner.Eval(block[0]) : null;
        }

        public static object If(Runner runner, Node block) {
            if(block.Count < 2 || block.Count > 3)
                throw new ArgumentException();
            if(RunnerHelper.IsTruly(runner.Eval(block[0])))
                return runner.Eval(block[1]);
            else if(block.Count == 3)
                return runner.Eval(block[2]);
            return null;
        }

        public static object While(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            object result = null;
            while(RunnerHelper.IsTruly(runner.Eval(block[0])))
                result = runner.Eval(block[1]);
            return result;
        }

        public static object Function(Runner runner, Node block) {
            if(block.Count < 3)
                throw new ArgumentException();
            FieldState field = null;
            var args = block[block.Count - 2];
            foreach(var child in block) {
                if(child == args) {
                    field.Value = block;
                    break;
                } else {
                    var v = Convert.ToString(runner.Eval(child));
                    field = field != null ?
                    field.GetSubField(runner, v) :
                    runner.GetField(v);
                }
            }
            return field.Value;
        }
    }
}
