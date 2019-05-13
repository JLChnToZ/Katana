using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class Runner {
        private static readonly Queue<Dictionary<string, Field>> unusedHeap =
            new Queue<Dictionary<string, Field>>();
        private static readonly Queue<Dictionary<Node, Field>> unusedCache =
            new Queue<Dictionary<Node, Field>>();
        private readonly Stack<Dictionary<string, Field>> heapStack =
            new Stack<Dictionary<string, Field>>();
        private readonly Stack<Dictionary<Node, Field>> cacheStack =
            new Stack<Dictionary<Node, Field>>();
        private readonly Node root;
        private readonly Dictionary<string, Field> globalHeapStack =
            new Dictionary<string, Field>();

        public object this[string key] {
            get => globalHeapStack.TryGetValue(key, out var field) ? field.Value : null;
            set => globalHeapStack[key] = new Field(value);
        }

        public Runner(Node root) {
            this.root = root;
            heapStack.Push(globalHeapStack);
            PushCache();
        }

        public Runner(string source) :
            this(Node.Deserialize(source)) {
        }

        public object Run() {
            return Eval(root).Value;
        }

        internal void PushContext() {
            if(unusedHeap.Count > 0)
                heapStack.Push(unusedHeap.Dequeue());
            else
                heapStack.Push(new Dictionary<string, Field>());
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
                cacheStack.Push(new Dictionary<Node, Field>());
        }

        internal void PopCache() {
            var cache = cacheStack.Pop();
            cache.Clear();
            unusedCache.Enqueue(cache);
        }

        public Field Eval(Node block) {
            if(block.Count == 0)
                return new Field(block.Tag);
            var cache = cacheStack.Peek();
            if(!cache.TryGetValue(block, out var result)) {
                var nextMap = new Dictionary<Node, int>();
                var lookupStack = new Stack<Node>();
                lookupStack.Push(block);
                while(lookupStack.Count > 0) {
                    var node = lookupStack.Peek();
                    nextMap.TryGetValue(node, out int next);
                    var hasFn = TryGetField(Convert.ToString(node.Tag), out var fn);
                    if(hasFn &&
                        fn.FieldType == FieldType.BuiltInFunction &&
                        !(fn.Value as BuiltInFunction).enableDefer) {
                        PushCache();
                        cache[node] = fn.Invoke(this, node);
                        PopCache();
                        lookupStack.Pop();
                        continue;
                    }
                    if(next >= node.Count) {
                        cache[node] = hasFn ? fn.Invoke(this, node) : default;
                        lookupStack.Pop();
                        continue;
                    }
                    lookupStack.Push(node[next]);
                    nextMap[node] = next + 1;
                }
                result = cache[block];
            } else
                cache.Remove(block);
            return result;
        }

        public bool TryGetField(string tag, out Field field, bool forceLocal = false) {
            if(heapStack.Peek().TryGetValue(tag ?? string.Empty, out field))
                return true;
            else if(!forceLocal && globalHeapStack.TryGetValue(tag, out field))
                return true;
            return false;
        }

        public Field GetFieldOrInit(string tag, bool forceLocal = false) {
            if(!TryGetField(tag, out Field field, forceLocal))
                heapStack.Peek()[tag] = field;
            return field;
        }

        public Field SetField(string tag, Field value, bool forceLocal = false) {
            var localHeap = heapStack.Peek();
            if(forceLocal || localHeap.ContainsKey(tag))
                localHeap[tag] = value;
            else
                globalHeapStack[tag] = value;
            return value;
        }
    }
}
