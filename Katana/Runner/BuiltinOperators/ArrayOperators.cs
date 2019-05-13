using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static Field CreateArray(Runner runner, Node block) {
            Field result = new Field(FieldType.Array, block.Count);
            foreach(var child in block)
                result.Add(runner.Eval(child));
            return result;
        }

        public static Field Any(Runner runner, Node block) {
            if(block.Count != 2)
                throw new ArgumentException();
            return runner.Eval(block[0]).Contains(runner.Eval(block[1]));
        }
    }
}
