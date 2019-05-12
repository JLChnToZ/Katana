using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public enum FieldType {
        Unassigned,
        Integer,
        Float,
        String,
        Object,
        Array,
        Function,
        BuiltInFunction,
    }

    internal struct SFieldState {
        public object value;
        public FieldType fieldType;
    }

    public class FieldState {
        private SFieldState state;

        public FieldType FieldType => state.fieldType;

        internal SFieldState State => state;

        public object Value {
            get => state.value;
            set {
                switch(state.fieldType = RunnerHelper.GetFieldType(value)) {
                    case FieldType.Unassigned:
                        state.value = null;
                        break;
                    default:
                        state.value = value;
                        break;
                }
            }
        }

        public long IntValue {
            get => Convert.ToInt64(state.value);
            set {
                state.fieldType = FieldType.Integer;
                state.value = value;
            }
        }

        public double FloatValue {
            get => Convert.ToDouble(state.value);
            set {
                state.fieldType = FieldType.Float;
                state.value = value;
            }
        }

        public string StringValue {
            get => Convert.ToString(state.value);
            set {
                state.fieldType = FieldType.String;
                state.value = value;
            }
        }

        public FieldState this[string tag] {
            get {
                switch(state.fieldType) {
                    case FieldType.Unassigned:
                        state.fieldType = FieldType.Object;
                        state.value = new Dictionary<string, FieldState>();
                        goto case FieldType.Object;
                    case FieldType.Object:
                        return GetSubFieldUnchecked(tag);
                    case FieldType.Array:
                        if(!int.TryParse(tag, out int index))
                            goto default;
                        return GetSubFieldUnchecked(index);
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public FieldState this[int index] {
            get {
                switch(state.fieldType) {
                    case FieldType.Unassigned:
                        state.fieldType = FieldType.Array;
                        state.value = new List<FieldState>();
                        goto case FieldType.Array;
                    case FieldType.Object:
                        var tag = index.ToString();
                        return GetSubFieldUnchecked(tag);
                    case FieldType.Array:
                        return GetSubFieldUnchecked(index);
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public int Count {
            get {
                switch(state.fieldType) {
                    case FieldType.String:
                        return (state.value as string).Length;
                    case FieldType.Array:
                        return (state.value as List<FieldState>).Count;
                    case FieldType.Object:
                        return (state.value as Dictionary<string, FieldState>).Count;
                    default:
                        return 0;
                }
            }
        }

        public FieldState() { }

        internal FieldState(SFieldState state) {
            this.state = state;
        }

        public bool Has(string tag) {
            switch(state.fieldType) {
                case FieldType.Object:
                    return (state.value as Dictionary<string, FieldState>).ContainsKey(tag);
                case FieldType.Array:
                    return int.TryParse(tag, out int index) &&
                        index >= 0 &&
                        index < (state.value as List<FieldState>).Count;
                default:
                    return false;
            }
        }

        public bool Has(int index) {
            switch(state.fieldType) {
                case FieldType.Object:
                    return (state.value as Dictionary<string, FieldState>).ContainsKey(index.ToString());
                case FieldType.Array:
                    return index >= 0 &&
                        index < (state.value as List<FieldState>).Count;
                default:
                    return false;
            }
        }

        private FieldState GetSubFieldUnchecked(string tag) {
            var fields = state.value as Dictionary<string, FieldState>;
            if(!fields.TryGetValue(tag, out var field)) {
                field = new FieldState();
                fields[tag] = field;
            }
            return field;
        }

        private FieldState GetSubFieldUnchecked(int index) {
            var fields = state.value as List<FieldState>;
            FieldState field;
            if(index > fields.Count) {
                if(fields.Capacity < index + 1)
                    fields.Capacity = index + 1;
                while(fields.Count <= index)
                    fields.Add(new FieldState());
                field = new FieldState();
                fields.Add(field);
            } else {
                if(index < 0)
                    index = (index % fields.Count + fields.Count) % fields.Count;
                field = fields[index];
            }
            return field;
        }

        internal SFieldState Call(Runner runner, Node node) {
            switch(state.fieldType) {
                case FieldType.BuiltInFunction:
                case FieldType.Function:
                    return (state.value as IFunction).Invoke(runner, node);
                default:
                    if(node.Count > 0)
                        throw new InvalidCastException();
                    return state;
            }
        }

        public void SetFunction(ScriptFunction fn) {
            state.fieldType = FieldType.Function;
            state.value = fn;
        }

        public void SetFunction(BuiltInFunction fn) {
            state.fieldType = FieldType.BuiltInFunction;
            state.value = fn;
        }
    }
}
