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
                var lookupStack = new Stack<(Node, int)>();
                lookupStack.Push((block, 0));
                while(lookupStack.Count > 0) {
                    var (node, next) = lookupStack.Pop();
                    if(node.Count == 0) {
                        ResolveNodeToCache(cache, node, new Field(node.Tag), false);
                        continue;
                    }
                    var hasFn = TryGetField(Convert.ToString(node), out var fn);
                    if(hasFn &&
                        fn.FieldType == FieldType.BuiltInFunction &&
                        !(fn.Value as BuiltInFunction).enableDefer) {
                        PushCache();
                        ResolveNodeToCache(cache, node, fn, true);
                        PopCache();
                        continue;
                    }
                    if(next >= node.Count) {
                        ResolveNodeToCache(cache, node, fn, true);
                        continue;
                    }
                    lookupStack.Push((node, next + 1));
                    lookupStack.Push((node[next], 0));
                }
                result = cache[block];
            }
            return result;
        }

        private void ResolveNodeToCache(Dictionary<Node, Field> cache, Node node, Field value, bool doInvoke) {
            cache[node] = doInvoke ? value.Invoke(this, node) : value;
            if(node.Count > 0)
                foreach(var child in node)
                    cache.Remove(child);
        }

        public bool TryGetField(string tag, out Field field, bool forceLocal = false) {
            if(heapStack.Peek().TryGetValue(tag ?? string.Empty, out field))
                return true;
            else if(!forceLocal && globalHeapStack.TryGetValue(tag, out field))
                return true;
            return false;
        }

        public Field GetFieldOrInit(string tag, FieldType ensureType = FieldType.Unassigned, bool forceLocal = false) {
            if(!TryGetField(tag, out Field field, forceLocal))
                heapStack.Peek()[tag] = field = new Field(ensureType);
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
