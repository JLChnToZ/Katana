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
        Function,
        BuiltInFunction,
    }

    public class FieldState {
        private object value;
        public FieldType fieldType;

        public object Value {
            get => value;
            set {
                switch(Convert.GetTypeCode(value)) {
                    case TypeCode.String:
                        this.value = value;
                        fieldType = FieldType.String;
                        break;
                    case TypeCode.DBNull:
                    case TypeCode.DateTime:
                    case TypeCode.Empty:
                        this.value = null;
                        fieldType = FieldType.Unassigned;
                        break;
                    case TypeCode.Object:
                        if(value is Dictionary<string, FieldState>) {
                            this.value = value;
                            fieldType = FieldType.Object;
                        }
                        if(value is Node) {
                            this.value = value;
                            fieldType = FieldType.Function;
                        }
                        if(value is BuiltInFunction) {
                            this.value = value;
                            fieldType = FieldType.BuiltInFunction;
                        }
                        goto case TypeCode.Empty;
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        this.value = Convert.ToDouble(value);
                        fieldType = FieldType.Float;
                        break;
                    default:
                        this.value = Convert.ToInt64(value);
                        fieldType = FieldType.Integer;
                        break;
                }
            }
        }
        
        public long GetIntegerValue(Runner runner) {
            switch(fieldType) {
                case FieldType.Integer:
                case FieldType.Float:
                    return Convert.ToInt64(value);
                case FieldType.String:
                    long.TryParse(value as string, out long x);
                    return x;
                default:
                    throw new InvalidCastException();
            }
        }

        public void SetIntegerValue(Runner runner, long value) {
            fieldType = FieldType.Integer;
            this.value = value;
        }

        public double GetFloatValue(Runner runner) {
            switch(fieldType) {
                case FieldType.Integer:
                case FieldType.Float:
                    return Convert.ToDouble(value);
                case FieldType.String:
                    long.TryParse(value as string, out long x);
                    return x;
                default:
                    throw new InvalidCastException();
            }
        }

        public void SetFloatValue(Runner runner, double value) {
            fieldType = FieldType.Float;
            this.value = value;
        }

        public string GetStringValue(Runner runner) {
            switch(fieldType) {
                case FieldType.String:
                    return value as string;
                case FieldType.Integer:
                case FieldType.Float:
                    return Convert.ToString(value);
                default:
                    throw new InvalidCastException();
            }
        }

        public void SetStringValue(Runner runner, string value) {
            fieldType = FieldType.String;
            this.value = value;
        }

        public object Call(Runner runner, Node node) {
            switch(fieldType) {
                case FieldType.BuiltInFunction:
                    return (value as BuiltInFunction).Invoke(runner, node);
                case FieldType.Function:
                    var fnBlock = value as Node;
                    runner.Push();
                    var args = fnBlock[fnBlock.Count - 2];
                    for(int i = 0, l = args.Count; i < l; i++) {
                        var arg = Convert.ToString(runner.Eval(args[i]));
                        runner.GetField(arg, true).value = node.Count > i ?
                            runner.Eval(node[i]) : null;
                    }
                    var result = runner.Eval(fnBlock[fnBlock.Count - 1]);
                    runner.Pop();
                    return result;
                default:
                    throw new InvalidCastException();
            }
        }

        public void SetFunction(Runner runner, Node node) {
            fieldType = FieldType.Function;
            value = node;
        }

        public void SetFunction(Runner runner, BuiltInFunction fn) {
            fieldType = FieldType.Function;
            value = fn;
        }

        public FieldState GetSubField(Runner runner, string tag) {
            var fields = value as Dictionary<string, FieldState>;
            if(fieldType == FieldType.Unassigned) {
                fieldType = FieldType.Object;
                value = fields = new Dictionary<string, FieldState>();
            } else if(fieldType != FieldType.Object)
                throw new InvalidCastException();
            if(!fields.TryGetValue(tag, out var field)) {
                field = new FieldState();
                fields[tag] = field;
            }
            return field;
        }
    }
}
