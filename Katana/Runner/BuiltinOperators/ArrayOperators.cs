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

        public static Field Delete(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var target = runner.Eval(block[0]);
            switch(target.FieldType) {
                case FieldType.Array:
                    switch(block.Count) {
                        case 3:
                            target.RemoveRange(
                                (int)runner.Eval(block[1]),
                                (int)runner.Eval(block[2])
                            );
                            break;
                        case 2:
                            target.RemoveAt((int)runner.Eval(block[1]));
                            break;
                        default:
                            throw new ArgumentException();
                    }
                    break;
                case FieldType.Object:
                    for(int i = 1, l = block.Count; i < l; i++)
                        target.Remove(runner.Eval(block[i]));
                    break;
                default:
                    throw new ArgumentException();
            }
            return target;
        }
    }
}
