using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static object CreateArray(Runner runner, Node block, out FieldType fieldType) {
            var result = new List<FieldState>(block.Count);
            foreach(var child in block) {
                var value = runner.Eval(child, out var ft);
                result.Add(new FieldState(new SFieldState {
                    fieldType = ft,
                    value = value,
                }));
            }
            fieldType = FieldType.Array;
            return result;
        }

        public static object Any(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 2)
                throw new ArgumentException();
            var list = runner.Eval(block[0], out var ft);
            if(ft != FieldType.Array)
                throw new ArgumentException();
            var entry = runner.Eval(block[1], out _);
            fieldType = FieldType.Integer;
            foreach(var field in list as List<FieldState>)
                if(field.Value == entry)
                    return 1;
            return 0;
        }
    }
}
