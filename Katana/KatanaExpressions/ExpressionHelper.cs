using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JLChnToZ.Katana.Expressions {
    internal static class ExpressionHelper {
        private static readonly Regex numberMatcher =
            new Regex("^(?:0x([0-9A-F]+)|(0[0-7]+)|([+-]?[0-9]+)|([-+]?[0-9]*\\.?[0-9]+(E[-+]?[0-9]+)?))$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private enum TokenMode {
            Expression,
            ExpressionAfterClosingQuote,
            ExpressionAfterString,
            Escape,
            InString,
            InStringEscape,
            InStringEscapeOct,
            InStringEscapeHex,
        }

        public static void Serialize(Node node, StringBuilder sb) {
            if(node == null) return;
            var stack = new Stack<(bool, IEnumerator<Node>)>();
            stack.Push((false, Enumerable.Repeat(node, 1).GetEnumerator()));
            bool firstLine = true;
            while(stack.Count > 0) {
                var (isOpenQuote, currentDepth) = stack.Pop();
                if(!currentDepth.MoveNext()) {
                    if(IndentAndLinebreak(ref firstLine, stack.Count, sb))
                        sb.Append(')');
                    continue;
                }
                var currentNode = currentDepth.Current;
                stack.Push((true, currentDepth));
                if(isOpenQuote)
                    sb.Append(',');
                IndentAndLinebreak(ref firstLine, stack.Count, sb);
                bool hasSerializeName = SerializeName(currentNode.Tag, sb);
                if(currentNode.Count <= 0)
                    continue;
                if(hasSerializeName)
                    sb.Append(' ');
                sb.Append('(');
                stack.Push((false, currentNode.GetEnumerator()));
            }
        }

        private static bool SerializeName(object src, StringBuilder sb) {
            if(src == null)
                return false;
            if(src.GetType().IsPrimitive && !(src is char)) {
                sb.Append(src);
                return true;
            }
            var srcStr = src.ToString();
            bool needWrap = srcStr == string.Empty;
            if(!needWrap)
                foreach(var c in srcStr) {
                    switch(c) {
                        case '(':
                        case ')':
                        case ',':
                        case '\"':
                        case '\'':
                        case '\\':
                            needWrap = true;
                            break;
                        default:
                            if(CanIgnoreChar(c))
                                needWrap = true;
                            break;
                    }
                    if(needWrap) break;
                }
            if(needWrap)
                SerializeNameWithQuotes(srcStr, sb);
            else
                sb.Append(srcStr);
            return true;
        }

        private static void SerializeNameWithQuotes(string src, StringBuilder sb) {
            sb.Append('\"');
            foreach(var c in src)
                switch(c) {
                    case '\"':
                    case '\'':
                    case '\\':
                        sb.Append($"\\{c}");
                        break;
                    case '\0':
                        sb.Append("\\0");
                        break;
                    case '\a':
                        sb.Append("\\a");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\x1b':
                        sb.Append("\\e");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\v':
                        sb.Append("\\v");
                        break;
                    default:
                        if(!char.IsControl(c))
                            sb.Append(c);
                        else if(c < 256)
                            sb.Append($"\\x{(int)c:X2}");
                        else
                            sb.Append($"\\u{(int)c:X4}");
                        break;
                }
            sb.Append('\"');
        }

        private static bool IndentAndLinebreak(ref bool firstLine, int indent, StringBuilder sb) {
            if(firstLine)
                firstLine = false;
            else
                sb.AppendLine();
            if(indent > 0) {
                sb.Append(' ', (indent - 1) * 2);
                return true;
            }
            return false;
        }

        public static Node Deserizlize(string source) {
            var mode = TokenMode.Expression;
            var nodeStack = new Stack<Node>();
            var sb = new List<char>();
            char stringPairChar = '\0';
            bool hasQuote = false;
            bool hasAddNode = false;
            bool isComments = false;
            int byteLength = 0;
            int escapeCharCode = 0;
            int escapeReturnOffset = 0;
            Node current;
            Node result = null;
            for(int i = 0; i < source.Length; i++) {
                char c = source[i];
                if(isComments) {
                    if(c == '\n')
                        isComments = false;
                    continue;
                }
                switch(mode) {
                    case TokenMode.Expression:
                        switch(c) {
                            default:
                                sb.Add(c);
                                break;
                            case '(':
                                current = CreateNode(sb, hasQuote, false);
                                if(nodeStack.Count > 0)
                                    nodeStack.Peek().Add(current);
                                nodeStack.Push(current);
                                if(result == null)
                                    result = nodeStack.Peek();
                                hasQuote = false;
                                hasAddNode = false;
                                break;
                            case ',':
                                if(nodeStack.Count < 1)
                                    throw new ArgumentException($"Unexpected '{c}'.");
                                nodeStack.Peek().Add(CreateNode(sb, hasQuote, false));
                                hasQuote = false;
                                hasAddNode = true;
                                break;
                            case ')':
                                if(nodeStack.Count < 1)
                                    throw new ArgumentException($"Unexpected '{c}'.");
                                current = CreateNode(sb, hasQuote, !hasAddNode);
                                var parent = nodeStack.Pop();
                                if(current != null)
                                    parent.Add(current);
                                hasQuote = false;
                                hasAddNode = false;
                                mode = TokenMode.ExpressionAfterClosingQuote;
                                break;
                            case '\'':
                            case '\"':
                                if(sb.Count > 0)
                                    foreach(var chr in sb)
                                        if(!CanIgnoreChar(chr))
                                            throw new ArgumentException($"Unexpected '{c}'.");
                                sb.Clear();
                                mode = TokenMode.InStringEscape;
                                stringPairChar = c;
                                hasQuote = true;
                                break;
                            case '\\':
                                mode = TokenMode.Escape;
                                break;
                            case ';':
                                isComments = true;
                                break;
                        }
                        break;
                    case TokenMode.ExpressionAfterClosingQuote:
                        if(CanIgnoreChar(c))
                            break;
                        switch(c) {
                            case ',':
                                mode = TokenMode.Expression;
                                hasQuote = false;
                                hasAddNode = true;
                                break;
                            case ')':
                                mode = TokenMode.Expression;
                                i--;
                                break;
                            case ';':
                                isComments = true;
                                break;
                            default:
                                throw new ArgumentException($"Unexpected '{c}'.");
                        }
                        break;
                    case TokenMode.ExpressionAfterString:
                        switch(c) {
                            case '(':
                            case ',':
                            case ')':
                                mode = TokenMode.Expression;
                                i--;
                                break;
                            case ';':
                                isComments = true;
                                break;
                            default:
                                if(!CanIgnoreChar(c))
                                    throw new ArgumentException($"Unexpected '{c}'.");
                                break;
                        }
                        break;
                    case TokenMode.Escape:
                        mode = TokenMode.Expression;
                        sb.Add(c);
                        break;
                    case TokenMode.InString:
                        switch(c) {
                            case '\'':
                            case '\"':
                                if(stringPairChar == c)
                                    mode = TokenMode.ExpressionAfterString;
                                else
                                    goto default;
                                break;
                            case '\\':
                                mode = TokenMode.InStringEscape;
                                break;
                            default:
                                sb.Add(c);
                                break;
                        }
                        break;
                    case TokenMode.InStringEscape:
                        mode = TokenMode.InString;
                        switch(c) {
                            default:
                                sb.Add(c);
                                break;
                            case 'a':
                                sb.Add('\a');
                                break;
                            case 'b':
                                sb.Add('\b');
                                break;
                            case 'e':
                                sb.Add('\x1b');
                                break;
                            case 'f':
                                sb.Add('\f');
                                break;
                            case 'n':
                                sb.Add('\n');
                                break;
                            case 'r':
                                sb.Add('\r');
                                break;
                            case 't':
                                sb.Add('\t');
                                break;
                            case 'v':
                                sb.Add('\v');
                                break;
                            case 'u':
                                mode = TokenMode.InStringEscapeHex;
                                escapeReturnOffset = i;
                                escapeCharCode = 0;
                                byteLength = 4;
                                break;
                            case 'U':
                                mode = TokenMode.InStringEscapeHex;
                                escapeReturnOffset = i;
                                escapeCharCode = 0;
                                byteLength = 8;
                                break;
                            case 'x':
                                mode = TokenMode.InStringEscapeHex;
                                escapeReturnOffset = i;
                                escapeCharCode = 0;
                                byteLength = 2;
                                break;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                                mode = TokenMode.InStringEscapeOct;
                                escapeReturnOffset = i;
                                escapeCharCode = c - '0';
                                break;
                        }
                        break;
                    case TokenMode.InStringEscapeOct:
                        if(!AppendOct(c, ref escapeCharCode)) {
                            mode = TokenMode.InStringEscape;
                            if(source[escapeReturnOffset] == '0') {
                                i = escapeReturnOffset + 1;
                                sb.Add('\0');
                            } else
                                i = escapeReturnOffset;
                        } else if(i - escapeCharCode >= 2) {
                            mode = TokenMode.InStringEscape;
                            sb.Add((char)escapeCharCode);
                        }
                        break;
                    case TokenMode.InStringEscapeHex:
                        if(!AppendHex(c, ref escapeCharCode)) {
                            mode = TokenMode.InStringEscape;
                            i = escapeReturnOffset;
                        } else if(i - escapeCharCode >= byteLength) {
                            mode = TokenMode.InStringEscape;
                            if(byteLength > 4)
                                sb.AddRange(char.ConvertFromUtf32(escapeCharCode));
                            else
                                sb.Add((char)escapeCharCode);
                        }
                        break;
                }
            }
            switch(mode) {
                case TokenMode.Expression:
                case TokenMode.ExpressionAfterClosingQuote:
                case TokenMode.ExpressionAfterString:
                    break;
                case TokenMode.Escape:
                    throw new ArgumentException("Escape does not resolve.");
                default:
                    throw new ArgumentException($"Missing '{stringPairChar}'");
            }
            if(result == null)
                result = CreateNode(sb, hasQuote, true);
            if(nodeStack.Count > 1)
                throw new ArgumentException("Missing ')'.");
            return result;
        }

        private static bool CanIgnoreChar(char c) =>
            char.IsWhiteSpace(c) || char.IsSeparator(c) || char.IsControl(c);

        private static Node CreateNode(List<char> sb, bool hasQuote, bool ignoreIfEmpty) {
            try {
                if(sb.Count <= 0)
                    return ignoreIfEmpty ? null : new Node(null);
                var str = new string(sb.ToArray());
                if(hasQuote)
                    return new Node(str);
                if(string.IsNullOrWhiteSpace(str))
                    return ignoreIfEmpty ? null : new Node(null);
                str = str.Trim();
                object result = str;
                try {
                    switch(str.ToLower()) {
                        case "nil":
                        case "null":
                            result = null;
                            break;
                        case "infinity":
                            result = double.PositiveInfinity;
                            break;
                        case "nan":
                            result = double.NaN;
                            break;
                        case "true":
                            result = true;
                            break;
                        case "false":
                            result = false;
                            break;
                        default:
                            var matches = numberMatcher.Match(str);
                            if(!matches.Success)
                                break;
                            var groups = matches.Groups;
                            if(groups[1].Length > 0)
                                result = Convert.ToInt64(groups[1].Value, 16);
                            else if(matches.Groups[2].Length > 0)
                                result = Convert.ToInt64(groups[2].Value, 8);
                            else if(matches.Groups[3].Length > 0)
                                result = Convert.ToInt64(groups[3].Value, 10);
                            else if(matches.Groups[4].Length > 0)
                                result = Convert.ToDouble(groups[4].Value);
                            break;
                    }
                } catch { }
                return new Node(result);
            } finally {
                sb.Clear();
            }
        }

        private static bool AppendOct(char c, ref int num) {
            unchecked {
                switch(c) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                        num = (num << 3) | (c - '0');
                        break;
                    default:
                        return false;
                }
            }
            return true;

        }

        private static bool AppendHex(char c, ref int num) {
            unchecked {
                switch(c) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        num = (num << 4) | (c - '0');
                        break;
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        num = (num << 4) | (c - 'A' + 10);
                        break;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        num = (num << 4) | (c - 'a' + 10);
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}