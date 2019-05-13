using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class BuiltInFunction: IFunction {
        public delegate Field Exec(Runner runner, Node node);

        public readonly Exec exec;
        public readonly bool enableDefer;

        public BuiltInFunction(Exec exec, bool enableDefer) {
            this.exec = exec;
            this.enableDefer = enableDefer;
        }

        Field IFunction.Invoke(Runner runner, Node node) {
            return exec.Invoke(runner, node);
        }
    }
}
