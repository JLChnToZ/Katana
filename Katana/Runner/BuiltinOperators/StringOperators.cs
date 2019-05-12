using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public static partial class BuiltinOperators {

        public static object Slice(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2 || block.Count > 3)
                throw new ArgumentException();
            fieldType = FieldType.String;
            var src = runner.Eval(block[0], out var ft);
            var start = Convert.ToInt32(runner.Eval(block[1], out _));
            var end = block.Count == 2 ?
                Convert.ToInt32(runner.Eval(block[2], out _)) : 0;
            if(ft == FieldType.Array) {
                var list = src as List<FieldState>;
                if(start < 0) start = list.Count + start;
                if(end <= 0) end = list.Count + end;
                var result = new List<FieldState>(end - start);
                for(int i = start; i < end; i++)
                    result[i - start] = new FieldState(list[i].State);
                return result;
            } else {
                var str = Convert.ToString(src);
                if(start < 0) start = str.Length + start;
                if(end <= 0) end = str.Length + end;
                return str.Substring(start, end - start);
            }
        }

        public static object ToChar(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            fieldType = FieldType.Integer;
            var str = Convert.ToString(runner.Eval(block[0], out _));
            var start = Convert.ToInt32(runner.Eval(block[1], out _));
            if(start < 0) start = str.Length + start;
            return (long)str[start];
        }

        public static object ToCharArray(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 2)
                throw new ArgumentException();
            fieldType = FieldType.Array;
            var str = Convert.ToString(runner.Eval(block[0], out _));
            var result = new List<FieldState>(str.Length);
            foreach(char c in str)
                result.Add(new FieldState { IntValue = c });
            return result;
        }

        public static object FromChar(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count < 1)
                throw new ArgumentException();
            var result = new char[block.Count];
            for(int i = 0, l = block.Count; i < l; i++)
                result[i] = Convert.ToChar(runner.Eval(block[0], out _));
            fieldType = FieldType.String;
            return new string(result);
        }

        public static object FromCharArray(Runner runner, Node block, out FieldType fieldType) {
            if(block.Count != 1)
                throw new ArgumentException();
            return FromChar(runner, block[1], out fieldType);
        }
    }
}
