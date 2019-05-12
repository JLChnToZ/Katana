using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class ScriptFunction: IFunction {
        public readonly List<Node> arguments;
        public readonly Node body;

        public ScriptFunction(IList<Node> arguments, Node body) {
            this.arguments = new List<Node>(arguments);
            this.body = body;
        }

        SFieldState IFunction.Invoke(Runner runner, Node node) {
            try {
                runner.PushContext();
                for(int i = 0, l = arguments.Count; i < l; i++) {
                    var arg = Convert.ToString(runner.Eval(arguments[i], out _));
                    runner.GetFieldOrInit(arg, true).Value = node.Count > i ?
                        runner.Eval(node[i], out _) : null;
                }
                var value = runner.Eval(body, out var fieldType);
                return new SFieldState {
                    value = value,
                    fieldType = fieldType,
                };
            } finally {
                runner.PopContext();
            }
        }
    }
}
