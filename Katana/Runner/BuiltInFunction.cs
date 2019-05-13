using System;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class BuiltInFunction: IFunction, IEquatable<BuiltInFunction> {
        public delegate Field Exec(Runner runner, Node node);

        public readonly Exec exec;
        public readonly bool enableDefer;

        public BuiltInFunction(Exec exec, bool enableDefer) {
            this.exec = exec;
            this.enableDefer = enableDefer;
        }

        public Field Invoke(Runner runner, Node node) =>
            exec.Invoke(runner, node);

        public override string ToString() =>
            exec.ToString();

        public override int GetHashCode() =>
            exec.GetHashCode() ^ enableDefer.GetHashCode();

        public override bool Equals(object obj) =>
            obj is BuiltInFunction other && Equals(other);

        public bool Equals(BuiltInFunction other) =>
            other != null &&
            exec == other.exec &&
            enableDefer == other.enableDefer;
    }
}
