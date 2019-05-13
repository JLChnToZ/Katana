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

        Field IFunction.Invoke(Runner runner, Node node) {
            try {
                runner.PushContext();
                for(int i = 0, l = arguments.Count; i < l; i++) {
                    var arg = runner.Eval(arguments[i]).StringValue;
                    runner.SetField(arg, node.Count > i ? runner.Eval(node[i]) : default);
                }
                var value = runner.Eval(body);
                return runner.Eval(body);
            } finally {
                runner.PopContext();
            }
        }
    }
}
