using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class ScriptFunction: IFunction, IEquatable<ScriptFunction> {
        public readonly Node arguments;
        public readonly Node body;

        public ScriptFunction(Node arguments, Node body) {
            this.arguments = arguments;
            this.body = body;
        }

        public Field Invoke(Runner runner, Node node) {
            try {
                runner.PushContext();
                for(int i = 0, l = Math.Min(arguments.Count, node.Count); i < l; i++)
                    runner.SetField(
                        runner.Eval(arguments[i]).StringValue,
                        runner.Eval(node[i]));
                return runner.Eval(body);
            } finally {
                runner.PopContext();
            }
        }

        public override string ToString() =>
            $"function ({arguments}, {body})";

        public override int GetHashCode() =>
            arguments.GetHashCode() ^ body.GetHashCode();

        public override bool Equals(object obj) =>
            obj is ScriptFunction other && Equals(other);

        public bool Equals(ScriptFunction other) =>
            other != null &&
            arguments.Equals(other.arguments) &&
            body.Equals(other.body);
    }
}
