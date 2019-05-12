using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class Runner {
        private static readonly Queue<Dictionary<string, FieldState>> unusedHeap =
            new Queue<Dictionary<string, FieldState>>();
        private static readonly Queue<Dictionary<Node, SFieldState>> unusedCache =
            new Queue<Dictionary<Node, SFieldState>>();
        private readonly Stack<Dictionary<string, FieldState>> heapStack =
            new Stack<Dictionary<string, FieldState>>();
        private readonly Stack<Dictionary<Node, SFieldState>> cacheStack =
            new Stack<Dictionary<Node, SFieldState>>();
        private readonly Node root;

        public Runner(Node root) {
            this.root = root;
            PushContext();
            PushCache();
        }

        public Runner(string source) :
            this(Node.Deserialize(source)) {
        }

        public object Run() {
            return Eval(root, out _);
        }

        internal void PushContext() {
            if(unusedHeap.Count > 0)
                heapStack.Push(unusedHeap.Dequeue());
            else
                heapStack.Push(new Dictionary<string, FieldState>());
        }

        internal void PopContext() {
            var heap = heapStack.Pop();
            heap.Clear();
            unusedHeap.Enqueue(heap);
        }

        internal void PushCache() {
            if(unusedCache.Count > 0)
                cacheStack.Push(unusedCache.Dequeue());
            else
                cacheStack.Push(new Dictionary<Node, SFieldState>());
        }

        internal void PopCache() {
            var cache = cacheStack.Pop();
            cache.Clear();
            unusedCache.Enqueue(cache);
        }

        internal object Eval(Node block, out FieldType fieldType) {
            if(block.Count == 0) {
                fieldType = RunnerHelper.GetFieldType(block.Tag);
                return block.Tag;
            }
            var cache = cacheStack.Peek();
            if(cache.TryGetValue(block, out var result)) {
                fieldType = result.fieldType;
                return result.value;
            }
            var nextMap = new Dictionary<Node, int>();
            var lookupStack = new Stack<Node>();
            lookupStack.Push(block);
            while(lookupStack.Count > 0) {
                var node = lookupStack.Peek();
                nextMap.TryGetValue(node, out int next);
                var fn = GetField(Convert.ToString(node.Tag));
                if(fn.FieldType == FieldType.BuiltInFunction &&
                    !(fn.Value as BuiltInFunction).enableDefer) {
                    PushCache();
                    cache[node] = fn.Call(this, node);
                    PopCache();
                    lookupStack.Pop();
                    continue;
                }
                if(next >= node.Count) {
                    cache[node] = EvalWithoutDigging(node);
                    lookupStack.Pop();
                    continue;
                }
                lookupStack.Push(node[next]);
                nextMap[node] = next + 1;
            }
            fieldType = FieldType.Unassigned;
            return null;
        }

        private SFieldState EvalWithoutDigging(Node node) {
            foreach(var fields in heapStack)
                if(fields.TryGetValue(Convert.ToString(node.Tag), out var field))
                    return field.Call(this, node);
            return default;
        }

        internal FieldState GetField(string tag, bool forceLocal = false) {
            FieldState field;
            foreach(var fields in heapStack)
                if(fields.TryGetValue(tag, out field))
                    return field;
                else if(forceLocal)
                    break;
            field = new FieldState();
            heapStack.Peek()[tag] = field;
            return field;
        }
    }
}
