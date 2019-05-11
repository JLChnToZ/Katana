using System;
using System.Collections.Generic;
using JLChnToZ.Katana.Expressions;

namespace JLChnToZ.Katana.Runner {
    public class Runner {
        private static readonly Queue<Dictionary<string, FieldState>> unusedHeap =
            new Queue<Dictionary<string, FieldState>>();
        private readonly Stack<Dictionary<string, FieldState>> heapStack =
            new Stack<Dictionary<string, FieldState>>();
        private readonly Node root;

        public Runner(Node root) {
            this.root = root;
            Push();
        }

        public Runner(string source) :
            this(Node.Deserialize(source)) {
        }

        public object Run() {
            return Eval(root);
        }

        internal void Push() {
            if(unusedHeap.Count > 0)
                heapStack.Push(unusedHeap.Dequeue());
            else
                heapStack.Push(new Dictionary<string, FieldState>());
        }

        internal void Pop() {
            var heap = heapStack.Pop();
            heap.Clear();
            unusedHeap.Enqueue(heap);
        }

        internal object Eval(Node block) {
            if(block.Count == 0)
                return block.Tag;
            foreach(var fields in heapStack)
                if(fields.TryGetValue(Convert.ToString(block.Tag), out var field))
                    return field.Call(this, block);
            return null;
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
