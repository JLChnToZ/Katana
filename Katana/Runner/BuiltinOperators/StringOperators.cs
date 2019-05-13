using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static Field Slice(Runner runner, Node block) {
            if(block.Count < 2 || block.Count > 3)
                throw new ArgumentException();
            var src = runner.Eval(block[0]);
            var start = (int)runner.Eval(block[1]);
            var end = block.Count == 2 ? (int)runner.Eval(block[2]) : 0;
            if(src.FieldType == FieldType.Array) {
                if(start < 0) start = src.Count + start;
                if(end <= 0) end = src.Count + end;
                Field result = new Field(FieldType.Array);
                result.Capacity = end - start;
                for(int i = start; i < end; i++)
                    result.Add(src[i]);
                return result;
            } else {
                var str = src.StringValue;
                if(start < 0) start = str.Length + start;
                if(end <= 0) end = str.Length + end;
                return str.Substring(start, end - start);
            }
        }

        public static Field ToChar(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var str = runner.Eval(block[0]);
            var start = (int)runner.Eval(block[1]);
            if(start < 0) start = str.Count + start;
            return (long)str[start];
        }

        public static Field ToCharArray(Runner runner, Node block) {
            if(block.Count < 2)
                throw new ArgumentException();
            var str = runner.Eval(block[0]).StringValue;
            Field result = new Field(FieldType.Array, str.Length);
            foreach(char c in str)
                result.Add(c);
            return result;
        }

        public static Field FromChar(Runner runner, Node block) {
            if(block.Count < 1)
                throw new ArgumentException();
            var result = new char[block.Count];
            for(int i = 0, l = block.Count; i < l; i++)
                result[i] = (char)runner.Eval(block[0]);
            return new string(result);
        }

        public static Field FromCharArray(Runner runner, Node block) {
            if(block.Count != 1)
                throw new ArgumentException();
            return FromChar(runner, block[0]);
        }
    }
}
