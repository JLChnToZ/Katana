using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    [StructLayout(LayoutKind.Explicit)]
    public struct Field: IEquatable<Field>, IConvertible, IFormattable, IFunction, IList<Field>, IDictionary<string, Field> {
        [FieldOffset(0)]
        private FieldType fieldType;
        [FieldOffset(4)]
        private long intValue;
        [FieldOffset(4)]
        private double floatValue;
        [FieldOffset(12)]
        private object objValue;

        public object Value {
            get {
                switch(fieldType) {
                    case FieldType.Unassigned: return null;
                    case FieldType.Integer: return intValue;
                    case FieldType.Float: return floatValue;
                    default: return objValue;
                }
            }
            set {
                switch(fieldType = RunnerHelper.GetFieldType(value)) {
                    case FieldType.Unassigned:
                        intValue = 0;
                        objValue = null;
                        break;
                    case FieldType.Integer:
                        intValue = Convert.ToInt64(value);
                        objValue = null;
                        break;
                    case FieldType.Float:
                        floatValue = Convert.ToDouble(value);
                        objValue = null;
                        break;
                    case FieldType.String:
                        intValue = 0;
                        objValue = Convert.ToString(value);
                        break;
                    default:
                        intValue = 0;
                        objValue = value;
                        break;
                }
            }
        }

        public FieldType FieldType => fieldType;

        bool ICollection<Field>.IsReadOnly => false;

        bool ICollection<KeyValuePair<string, Field>>.IsReadOnly => false;

        public int Count {
            get {
                switch(fieldType) {
                    case FieldType.Array:
                    case FieldType.Object:
                        return (objValue as ICollection).Count;
                    case FieldType.String:
                        return (objValue as string).Length;
                    default:
                        return 0;
                }
            }
        }

        public int Capacity {
            get {
                switch(fieldType) {
                    case FieldType.Unassigned:
                    case FieldType.Array:
                        return ListValue.Capacity;
                    default:
                        return 0;
                }
            }
            set {
                switch(fieldType) {
                    case FieldType.Unassigned:
                        fieldType = FieldType.Array;
                        objValue = new List<Field>(value);
                        break;
                    case FieldType.Array:
                        if(ListValue.Capacity < value)
                            ListValue.Capacity = value;
                        break;
                }
            }
        }

        public Field this[string tag] {
            get {
                switch(fieldType) {
                    case FieldType.Unassigned:
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
            set {
                switch(fieldType) {
                    case FieldType.Unassigned:
                    case FieldType.Object:
                        GetSubFieldUnchecked(tag);
                        HashValue[tag] = value;
                        break;
                    case FieldType.Array:
                        if(!int.TryParse(tag, out int index))
                            goto default;
                        GetSubFieldUnchecked(index);
                        ListValue[index] = value;
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public Field this[int index] {
            get {
                switch(fieldType) {
                    case FieldType.Unassigned:
                    case FieldType.Object:
                        var tag = index.ToString();
                        return GetSubFieldUnchecked(tag);
                    case FieldType.Array:
                        return GetSubFieldUnchecked(index);
                    default:
                        throw new InvalidCastException();
                }
            }
            set {
                switch(fieldType) {
                    case FieldType.Unassigned:
                    case FieldType.Array:
                        GetSubFieldUnchecked(index);
                        ListValue[index] = value;
                        break;
                    case FieldType.Object:
                        var tag = index.ToString();
                        GetSubFieldUnchecked(tag);
                        HashValue[tag] = value;
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public Field this[Field field] {
            get {
                switch(field.FieldType) {
                    case FieldType.Integer:
                    case FieldType.Float:
                        return this[(int)field];
                    case FieldType.String:
                        return this[field.StringValue];
                    default:
                        throw new InvalidCastException();
                }
            }
            set {
                switch(field.FieldType) {
                    case FieldType.Integer:
                    case FieldType.Float:
                        this[(int)field] = value;
                        break;
                    case FieldType.String:
                        this[field.StringValue] = value;
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public Dictionary<string, Field> HashValue {
            get {
                if(fieldType == FieldType.Unassigned) {
                    fieldType = FieldType.Object;
                    objValue = new Dictionary<string, Field>();
                } else if(fieldType != FieldType.Object)
                    throw new InvalidCastException();
                return objValue as Dictionary<string, Field>;
            }
        }

        public List<Field> ListValue {
            get {
                if(fieldType == FieldType.Unassigned) {
                    fieldType = FieldType.Array;
                    objValue = new List<Field>();
                } else if(fieldType != FieldType.Array)
                    throw new InvalidCastException();
                return objValue as List<Field>;
            }
        }

        public string StringValue {
            get => (this as IConvertible).ToString(null);
        }

        public long IntValue {
            get => (this as IConvertible).ToInt64(null);
        }

        public double FloatValue {
            get => (this as IConvertible).ToDouble(null);
        }

        public bool IsTruly {
            get => (this as IConvertible).ToBoolean(null);
        }

        public Field GetAndEnsureType(Field key, FieldType type) {
            var result = this[key];
            switch(type) {
                case FieldType.Array:
                    if(result.fieldType == FieldType.Unassigned)
                        this[key] = result = new Field(new List<Field>());
                    break;
                case FieldType.Object:
                    if(result.fieldType == FieldType.Unassigned)
                        this[key] = result = new Field(new Dictionary<string, Field>());
                    break;
            }
            return result;
        }

        public ICollection<string> Keys {
            get {
                switch(fieldType) {
                    case FieldType.Object:
                        return HashValue.Keys;
                    default:
                        return null;
                }
            }
        }

        public ICollection<Field> Values {
            get {
                switch(fieldType) {
                    case FieldType.Object:
                        return HashValue.Values;
                    case FieldType.Array:
                        return ListValue;
                    default:
                        return null;
                }
            }
        }

        #region Constructors
        internal Field(object value) {
            floatValue = 0;
            intValue = 0;
            objValue = null;
            fieldType = RunnerHelper.GetFieldType(value);
            switch(fieldType) {
                case FieldType.Unassigned: break;
                case FieldType.Float:
                    floatValue = Convert.ToDouble(value); break;
                case FieldType.Integer:
                    intValue = Convert.ToInt64(value); break;
                default: objValue = value; break;
            }
        }

        public Field(string value) {
            floatValue = 0;
            intValue = 0;
            objValue = value;
            fieldType = value == null ?
                FieldType.Unassigned :
                FieldType.String;
        }

        public Field(long value) {
            floatValue = 0;
            intValue = value;
            objValue = null;
            fieldType = FieldType.Integer;
        }

        public Field(double value) {
            intValue = 0;
            floatValue = value;
            objValue = null;
            fieldType = FieldType.Float;
        }

        public Field(BuiltInFunction value) {
            floatValue = 0;
            intValue = 0;
            objValue = value;
            fieldType = value == null ?
                FieldType.Unassigned :
                FieldType.BuiltInFunction;
        }

        public Field(ScriptFunction value) {
            floatValue = 0;
            intValue = 0;
            objValue = value;
            fieldType = value == null ?
                FieldType.Unassigned :
                FieldType.Function;
        }

        public Field(List<Field> value) {
            floatValue = 0;
            intValue = 0;
            objValue = value;
            fieldType = value == null ?
                FieldType.Unassigned :
                FieldType.Array;
        }

        public Field(Dictionary<string, Field> value) {
            floatValue = 0;
            intValue = 0;
            objValue = value;
            fieldType = value == null ?
                FieldType.Unassigned :
                FieldType.Object;
        }
        #endregion

        #region Internal Helpers
        private Field GetSubFieldUnchecked(string tag) {
            var fields = HashValue;
            if(!fields.TryGetValue(tag, out var field))
                fields[tag] = field;
            return field;
        }

        private Field GetSubFieldUnchecked(int index) {
            var fields = ListValue;
            Field field;
            if(EnsuereRange(ref index)) {
                field = fields[index];
            } else if(index >= 0) {
                if(fields.Capacity < index + 1)
                    fields.Capacity = index + 1;
                while(fields.Count <= index)
                    fields.Add(new Field());
                field = new Field();
                fields.Add(field);
            } else
                throw new IndexOutOfRangeException();
            return field;
        }

        private bool EnsuereRange(ref int index) {
            var list = ListValue;
            int count = list.Count;
            if(index < 0)
                index += count;
            return index > 0 && index < count;
        }
        #endregion

        #region Callable
        public Field Invoke(Runner runner, Node node) {
            switch(fieldType) {
                case FieldType.BuiltInFunction:
                case FieldType.Function:
                    return (objValue as IFunction).Invoke(runner, node);
                default:
                    if(node.Count > 0)
                        throw new InvalidCastException();
                    return this;
            }
        }
        #endregion

        #region Convertible
        TypeCode IConvertible.GetTypeCode() {
            switch(fieldType) {
                case FieldType.String:
                    return TypeCode.String;
                case FieldType.Integer:
                    return TypeCode.Int64;
                case FieldType.Float:
                    return TypeCode.Double;
                case FieldType.Array:
                case FieldType.Object:
                case FieldType.Function:
                case FieldType.BuiltInFunction:
                    return TypeCode.Object;
                default:
                    return TypeCode.Empty;
            }
        }

        bool IConvertible.ToBoolean(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(bool.TryParse(objValue as string, out var result))
                        return result;
                    return !string.IsNullOrEmpty(objValue as string);
                case FieldType.Integer:
                    return intValue != 0;
                case FieldType.Float:
                    return floatValue != 0 && !double.IsNaN(floatValue);
                case FieldType.Array:
                case FieldType.Object:
                case FieldType.Function:
                case FieldType.BuiltInFunction:
                    return true;
                default:
                    return false;
            }
        }

        byte IConvertible.ToByte(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(byte.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((byte)intValue);
                case FieldType.Float:
                    return unchecked((byte)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(sbyte.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((sbyte)intValue);
                case FieldType.Float:
                    return unchecked((sbyte)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        char IConvertible.ToChar(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(char.TryParse(objValue as string, out var chr))
                        return chr;
                    if(ushort.TryParse(objValue as string, out ushort int16))
                        return (char)int16;
                    goto default;
                case FieldType.Integer:
                    return unchecked((char)intValue);
                case FieldType.Float:
                    return unchecked((char)floatValue);
                case FieldType.Unassigned:
                    return '\0';
                default:
                    throw new InvalidCastException();
            }
        }

        short IConvertible.ToInt16(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(short.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((short)intValue);
                case FieldType.Float:
                    return unchecked((short)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(ushort.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((ushort)intValue);
                case FieldType.Float:
                    return unchecked((ushort)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        int IConvertible.ToInt32(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(int.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((int)intValue);
                case FieldType.Float:
                    return unchecked((int)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        uint IConvertible.ToUInt32(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(uint.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((uint)intValue);
                case FieldType.Float:
                    return unchecked((uint)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        long IConvertible.ToInt64(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(long.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return intValue;
                case FieldType.Float:
                    return unchecked((long)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(ulong.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return unchecked((ulong)intValue);
                case FieldType.Float:
                    return unchecked((ulong)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(decimal.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return intValue;
                case FieldType.Float:
                    return (decimal)floatValue;
                case FieldType.Unassigned:
                    return 0;
                default:
                    throw new InvalidCastException();
            }
        }

        float IConvertible.ToSingle(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(float.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return intValue;
                case FieldType.Float:
                    return unchecked((float)floatValue);
                case FieldType.Unassigned:
                    return 0;
                default:
                    return float.NaN;
            }
        }

        double IConvertible.ToDouble(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    if(double.TryParse(objValue as string, out var result))
                        return result;
                    goto default;
                case FieldType.Integer:
                    return intValue;
                case FieldType.Float:
                    return floatValue;
                case FieldType.Unassigned:
                    return 0;
                default:
                    return double.NaN;
            }
        }

        public override string ToString() {
            switch(fieldType) {
                case FieldType.String:
                    return objValue as string;
                case FieldType.Integer:
                    return intValue.ToString();
                case FieldType.Float:
                    return floatValue.ToString();
                case FieldType.Unassigned:
                    return string.Empty;
                default:
                    return Convert.ToString(objValue);
            }
        }

        public string ToString(IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    return objValue as string;
                case FieldType.Integer:
                    return intValue.ToString(provider);
                case FieldType.Float:
                    return floatValue.ToString(provider);
                case FieldType.Unassigned:
                    return string.Empty;
                default:
                    return Convert.ToString(objValue, provider);
            }
        }

        public string ToString(string format, IFormatProvider provider) {
            switch(fieldType) {
                case FieldType.String:
                    return objValue as string;
                case FieldType.Integer:
                    return intValue.ToString(format, provider);
                case FieldType.Float:
                    return floatValue.ToString(format, provider);
                case FieldType.Unassigned:
                    return string.Empty;
                default:
                    return Convert.ToString(objValue, provider);
            }
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider) {
            throw new InvalidCastException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
            switch(Type.GetTypeCode(conversionType)) {
                case TypeCode.Boolean: return (this as IConvertible).ToBoolean(provider);
                case TypeCode.Byte: return (this as IConvertible).ToByte(provider);
                case TypeCode.SByte: return (this as IConvertible).ToSByte(provider);
                case TypeCode.Int16: return (this as IConvertible).ToInt16(provider);
                case TypeCode.Int32: return (this as IConvertible).ToInt32(provider);
                case TypeCode.Int64: return (this as IConvertible).ToInt64(provider);
                case TypeCode.UInt16: return (this as IConvertible).ToUInt16(provider);
                case TypeCode.UInt32: return (this as IConvertible).ToUInt32(provider);
                case TypeCode.UInt64: return (this as IConvertible).ToUInt64(provider);
                case TypeCode.Single: return (this as IConvertible).ToSingle(provider);
                case TypeCode.Double: return (this as IConvertible).ToDouble(provider);
                case TypeCode.Decimal: return (this as IConvertible).ToDecimal(provider);
                case TypeCode.String: return (this as IConvertible).ToString(provider);
                case TypeCode.Empty:
                    if(fieldType == FieldType.Unassigned)
                        return null;
                    break;
                case TypeCode.DBNull:
                    if(fieldType == FieldType.Unassigned)
                        return DBNull.Value;
                    break;
                case TypeCode.Object:
                    if(fieldType == FieldType.Unassigned) {
                        if(conversionType.IsValueType) break;
                        return null;
                    }
                    if(conversionType.IsAssignableFrom(objValue.GetType()))
                        return objValue;
                    break;
            }
            return Convert.ChangeType(objValue, conversionType, provider);
        }
        #endregion

        #region Collections
        public bool Contains(Field item) {
            switch(fieldType) {
                case FieldType.Object:
                    return HashValue.ContainsValue(item);
                case FieldType.Unassigned:
                case FieldType.Array:
                    return ListValue.Contains(item);
                case FieldType.String:
                    switch(item.fieldType) {
                        case FieldType.Integer:
                        case FieldType.Float:
                        case FieldType.String:
                            return StringValue.Contains((string)item);
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        bool ICollection<KeyValuePair<string, Field>>.Contains(KeyValuePair<string, Field> item) {
            if(fieldType == FieldType.Object)
                return (HashValue as IDictionary<string, Field>).Contains(item);
            return false;
        }

        public bool ContainsKey(string key) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Object:
                    return HashValue.ContainsKey(key);
                case FieldType.Array:
                    return int.TryParse(key, out int index) && index >= 0 && index < ListValue.Count;
                case FieldType.String:
                    return StringValue.Contains(key);
                default:
                    return false;
            }
        }

        public int IndexOf(Field item) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Array:
                    return ListValue.IndexOf(item);
                case FieldType.String:
                    switch(item.fieldType) {
                        case FieldType.Integer:
                        case FieldType.Float:
                        case FieldType.String:
                            return (objValue as string).IndexOf((string)item);
                        default:
                            return -1;
                    }
                default:
                    return -1;
            }
        }

        public void Add(Field item) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Array:
                    ListValue.Add(item);
                    break;
            }
            throw new NotSupportedException();
        }

        public void Add(string key, Field value) {
            switch(fieldType) {
                case FieldType.Array:

                    break;
                case FieldType.Unassigned:
                case FieldType.Object:
                    HashValue.Add(key, value);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        void ICollection<KeyValuePair<string, Field>>.Add(KeyValuePair<string, Field> item) =>
            Add(item.Key, item.Value);

        public void Insert(int index, Field item) {
            switch(fieldType) {
                case FieldType.Array:
                    ListValue.Insert(index, item);
                    break;
                case FieldType.Object:
                    HashValue.Add(index.ToString(), item);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public bool TryGetValue(string key, out Field value) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Object:
                    return HashValue.TryGetValue(key, out value);
                case FieldType.Array:
                    if(!int.TryParse(key, out int index) || !EnsuereRange(ref index)) {
                        value = default;
                        return false;
                    }
                    value = ListValue[index];
                    return true;
                default:
                    value = default;
                    return false;
            }
        }

        public bool Remove(Field item) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Array:
                    return ListValue.Remove(item);
                default:
                    return false;
            }
        }

        public bool Remove(string key) {
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Object:
                    return HashValue.Remove(key);
                case FieldType.Array:
                    if(!int.TryParse(key, out int index) || !EnsuereRange(ref index))
                        return false;
                    ListValue.RemoveAt(index);
                    return true;
                default:
                    return false;
            }
        }

        public void RemoveAt(int index) {
            switch(fieldType) {
                case FieldType.Object:
                    HashValue.Remove(index.ToString());
                    break;
                case FieldType.Unassigned:
                case FieldType.Array:
                    EnsuereRange(ref index);
                    ListValue.RemoveAt(index);
                    break;
                case FieldType.String:
                    objValue = StringValue.Remove(index, 1);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        bool ICollection<KeyValuePair<string, Field>>.Remove(KeyValuePair<string, Field> item) {
            if(fieldType == FieldType.Object)
                return (HashValue as IDictionary<string, Field>).Remove(item);
            return false;
        }

        public void Clear() {
            switch(fieldType) {
                case FieldType.Object:
                    HashValue.Clear();
                    break;
                case FieldType.Array:
                    ListValue.Clear();
                    break;
                case FieldType.String:
                    objValue = string.Empty;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void CopyTo(Field[] array, int arrayIndex) {
            switch(fieldType) {
                case FieldType.Object:
                    HashValue.Values.CopyTo(array, arrayIndex);
                    break;
                case FieldType.Unassigned:
                case FieldType.Array:
                    ListValue.CopyTo(array, arrayIndex);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        void ICollection<KeyValuePair<string, Field>>.CopyTo(KeyValuePair<string, Field>[] array, int arrayIndex) {
            if(fieldType == FieldType.Object)
                (HashValue as IDictionary<string, Field>).CopyTo(array, arrayIndex);
            else
                throw new NotSupportedException();
        }

        public IEnumerator<Field> GetEnumerator() {
            switch(fieldType) {
                case FieldType.Array:
                    return ListValue.GetEnumerator();
                case FieldType.Object:
                    return HashValue.Values.GetEnumerator();
                case FieldType.String:
                    return StringValue.Select(Char2Field).GetEnumerator();
                default:
                    return Enumerable.Empty<Field>().GetEnumerator();
            }
        }

        IEnumerator<KeyValuePair<string, Field>> IEnumerable<KeyValuePair<string, Field>>.GetEnumerator() {
            if(fieldType == FieldType.Object)
                return HashValue.GetEnumerator();
            return Enumerable.Empty<KeyValuePair<string, Field>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            (this as IEnumerable<Field>).GetEnumerator();

        private static Field Char2Field(char c) {
            return new Field(c.ToString());
        }
        #endregion

        #region Compare
        public bool Equals(Field other) =>
            fieldType == other.fieldType && Equals(objValue, other.objValue);

        public override bool Equals(object obj) =>
            obj is Field field && Equals(field);

        public override int GetHashCode() =>
            objValue != null ?
                objValue.GetHashCode() :
                fieldType.GetHashCode();
        #endregion

        #region Cast
        public static implicit operator Field(string value) =>
            new Field(value);

        public static implicit operator Field(bool value) =>
            new Field(value ? 1 : 0);

        public static implicit operator Field(long value) =>
            new Field(value);

        public static implicit operator Field(double value) =>
            new Field(value);

        public static implicit operator Field(BuiltInFunction value) =>
            new Field(value);

        public static implicit operator Field(ScriptFunction value) =>
            new Field(value);

        public static explicit operator string(Field value) =>
            (value as IConvertible).ToString(null);

        public static explicit operator bool(Field value) =>
            (value as IConvertible).ToBoolean(null);

        public static explicit operator byte(Field value) =>
            (value as IConvertible).ToByte(null);

        public static explicit operator sbyte(Field value) =>
            (value as IConvertible).ToSByte(null);

        public static explicit operator char(Field value) =>
            (value as IConvertible).ToChar(null);

        public static explicit operator short(Field value) =>
            (value as IConvertible).ToInt16(null);

        public static explicit operator ushort(Field value) =>
            (value as IConvertible).ToUInt16(null);

        public static explicit operator int(Field value) =>
            (value as IConvertible).ToInt32(null);

        public static explicit operator uint(Field value) =>
            (value as IConvertible).ToUInt32(null);

        public static explicit operator long(Field value) =>
            (value as IConvertible).ToInt64(null);

        public static explicit operator ulong(Field value) =>
            (value as IConvertible).ToUInt64(null);

        public static explicit operator float(Field value) =>
            (value as IConvertible).ToSingle(null);

        public static explicit operator double(Field value) =>
            (value as IConvertible).ToDouble(null);
        #endregion
    }
}
