using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class BuiltInFunction: IFunction {
        public delegate object Exec(Runner runner, Node node, out FieldType fieldType);

        public readonly Exec exec;
        public readonly bool enableDefer;

        public BuiltInFunction(Exec exec, bool enableDefer) {
            this.exec = exec;
            this.enableDefer = enableDefer;
        }

        SFieldState IFunction.Invoke(Runner runner, Node node) {
            var value = exec.Invoke(runner, node, out var fieldType);
            return new SFieldState {
                value = value,
                fieldType = fieldType,
            };
        }
    }
}
