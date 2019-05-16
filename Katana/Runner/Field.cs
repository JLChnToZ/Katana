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
        private readonly FieldType fieldType;
        [FieldOffset(4)]
        private readonly long intValue;
        [FieldOffset(4)]
        private readonly double floatValue;
        [FieldOffset(12)]
        private readonly object objValue;

        public object Value {
            get {
                switch(fieldType) {
                    case FieldType.Unassigned: return null;
                    case FieldType.Integer: return intValue;
                    case FieldType.Float: return floatValue;
                    default: return objValue;
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
                    case FieldType.Array:
                        if(ListValue.Capacity < value)
                            ListValue.Capacity = value;
                        break;
                    default:
                        throw new InvalidCastException();
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
                if(fieldType != FieldType.Object)
                    throw new InvalidCastException();
                return objValue as Dictionary<string, Field>;
            }
        }

        public List<Field> ListValue {
            get {
                if(fieldType != FieldType.Array)
                    throw new InvalidCastException();
                return objValue as List<Field>;
            }
        }

        public string StringValue => ToString();

        public long IntValue => ToInt64();

        public double FloatValue => ToDouble();

        public bool IsTruly => ToBoolean();

        public Field GetAndEnsureType(Field key, FieldType type) {
            var result = this[key];
            if(result.fieldType == FieldType.Unassigned)
                this[key] = result = new Field(type);
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
            if(value is Field field) {
                intValue = field.intValue;
                floatValue = field.floatValue;
                objValue = field.objValue;
                fieldType = field.fieldType;
                return;
            }
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

        public Field(FieldType fieldType, int capacity = 4) {
            this.fieldType = fieldType;
            floatValue = 0;
            intValue = 0;
            switch(fieldType) {
                case FieldType.Unassigned:
                case FieldType.Integer:
                case FieldType.Float:
                    objValue = null;
                    break;
                case FieldType.String:
                    objValue = string.Empty;
                    break;
                case FieldType.Object:
                    objValue = new Dictionary<string, Field>(capacity);
                    break;
                case FieldType.Array:
                    objValue = new List<Field>(capacity);
                    break;
                default:
                    throw new NotSupportedException();
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

        public Field(IEnumerable<Field> value) {
            if(value is Field field) {
                intValue = field.intValue;
                floatValue = field.floatValue;
                objValue = field.objValue;
                fieldType = field.fieldType;
                return;
            }
            floatValue = 0;
            intValue = 0;
            objValue = value as List<Field> ??
                new List<Field>(value);
            fieldType = FieldType.Array;
        }

        public Field(IDictionary<string, Field> value) {
            if(value is Field field) {
                intValue = field.intValue;
                floatValue = field.floatValue;
                objValue = field.objValue;
                fieldType = field.fieldType;
                return;
            }
            floatValue = 0;
            intValue = 0;
            objValue = value as Dictionary<string, Field> ??
                new Dictionary<string, Field>(value);
            fieldType = FieldType.Object;
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
                case FieldType.Array:
                case FieldType.Object:
                    var value = this;
                    for(int i = 1, l = node.Count; i < l; i++)
                        value = value.GetAndEnsureType(
                            runner.Eval(node[i]),
                            i < l - 1 ? FieldType.Object : FieldType.Unassigned);
                    return value;
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

        private bool ToBoolean() {
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

        bool IConvertible.ToBoolean(IFormatProvider provider) => ToBoolean();

        private byte ToByte() {
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

        byte IConvertible.ToByte(IFormatProvider provider) => ToByte();

        private sbyte ToSByte() {
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

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ToSByte();

        private char ToChar() {
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

        char IConvertible.ToChar(IFormatProvider provider) => ToChar();

        private short ToInt16() {
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

        short IConvertible.ToInt16(IFormatProvider provider) => ToInt16();

        private ushort ToUInt16() {
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

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ToUInt16();

        private int ToInt32() {
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

        int IConvertible.ToInt32(IFormatProvider provider) => ToInt32();

        private uint ToUInt32() {
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

        uint IConvertible.ToUInt32(IFormatProvider provider) => ToUInt32();

        private long ToInt64() {
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

        long IConvertible.ToInt64(IFormatProvider provider) => ToInt64();

        private ulong ToUInt64() {
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

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ToUInt64();

        private decimal ToDecimal() {
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

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ToUInt64();

        private float ToSingle() {
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

        float IConvertible.ToSingle(IFormatProvider provider) => ToSingle();

        private double ToDouble() {
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

        double IConvertible.ToDouble(IFormatProvider provider) => ToDouble();

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

        DateTime IConvertible.ToDateTime(IFormatProvider provider) =>
            throw new InvalidCastException();

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
            switch(Type.GetTypeCode(conversionType)) {
                case TypeCode.Boolean: return ToBoolean();
                case TypeCode.Byte: return ToByte();
                case TypeCode.SByte: return ToSByte();
                case TypeCode.Int16: return ToInt16();
                case TypeCode.Int32: return ToInt32();
                case TypeCode.Int64: return ToInt64();
                case TypeCode.UInt16: return ToUInt16();
                case TypeCode.UInt32: return ToUInt32();
                case TypeCode.UInt64: return ToUInt64();
                case TypeCode.Single: return ToSingle();
                case TypeCode.Double: return ToDouble();
                case TypeCode.Decimal: return ToDecimal();
                case TypeCode.String: return ToString(provider);
                case TypeCode.DateTime:
                    throw new InvalidCastException();
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
                case FieldType.Array:
                    ListValue.Add(item);
                    break;
            }
            throw new NotSupportedException();
        }

        public void Add(string key, Field value) {
            switch(fieldType) {
                case FieldType.Array:
                    if(int.TryParse(key, out int index) && EnsuereRange(ref index))
                        ListValue[index] = value;
                    else
                        throw new IndexOutOfRangeException();
                    break;
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
                case FieldType.Array:
                    return ListValue.Remove(item);
                default:
                    return false;
            }
        }

        public bool Remove(string key) {
            switch(fieldType) {
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
                case FieldType.Array:
                    EnsuereRange(ref index);
                    ListValue.RemoveAt(index);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void RemoveRange(int index, int count) {
            switch(fieldType) {
                case FieldType.Array:
                    ListValue.RemoveRange(index, count);
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
                default:
                    throw new NotSupportedException();
            }
        }

        public void CopyTo(Field[] array, int arrayIndex) {
            switch(fieldType) {
                case FieldType.Object:
                    HashValue.Values.CopyTo(array, arrayIndex);
                    break;
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static Field Char2Field(char c) {
            return new Field(c.ToString());
        }
        #endregion

        #region Compare
        public bool Equals(Field other) {
            if(fieldType != other.fieldType)
                return false;
            switch(fieldType) {
                case FieldType.Unassigned:
                    return true;
                case FieldType.Integer:
                    return intValue == other.intValue;
                case FieldType.Float:
                    return floatValue == other.floatValue;
                default:
                    return Equals(objValue, other.objValue);
            }
        }

        public override bool Equals(object obj) =>
            obj is Field field && Equals(field);

        public override int GetHashCode() {
            switch(fieldType) {
                case FieldType.Unassigned:
                    return 0;
                case FieldType.Integer:
                    return intValue.GetHashCode();
                case FieldType.Float:
                    return floatValue.GetHashCode();
                default:
                    return objValue.GetHashCode();
            }
        }
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

        public static implicit operator Field(List<Field> value) =>
            new Field(value);

        public static implicit operator Field(Dictionary<string, Field> value) =>
            new Field(value);

        public static explicit operator string(Field value) =>
            value.ToString();

        public static explicit operator bool(Field value) =>
            value.ToBoolean();

        public static explicit operator byte(Field value) =>
            value.ToByte();

        public static explicit operator sbyte(Field value) =>
            value.ToSByte();

        public static explicit operator char(Field value) =>
            value.ToChar();

        public static explicit operator short(Field value) =>
            value.ToInt16();

        public static explicit operator ushort(Field value) =>
            value.ToUInt16();

        public static explicit operator int(Field value) =>
            value.ToInt32();

        public static explicit operator uint(Field value) =>
            value.ToUInt32();

        public static explicit operator long(Field value) =>
            value.ToInt64();

        public static explicit operator ulong(Field value) =>
            value.ToUInt64();

        public static explicit operator float(Field value) =>
            value.ToSingle();

        public static explicit operator double(Field value) =>
            value.ToDouble();
        #endregion
    }
}
