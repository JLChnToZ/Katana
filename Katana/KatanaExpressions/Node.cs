﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JLChnToZ.Katana.Expressions {
    public partial class Node: IList<Node>, ICloneable {
        private List<Node> nodes;

        public object Tag { get; }

        public int Count => nodes == null ? 0 : nodes.Count;

        bool ICollection<Node>.IsReadOnly => false;

        public Node this[int index] {
            get => nodes == null ?
                throw new ArgumentOutOfRangeException(nameof(index)) :
                nodes[index];
            set {
                if(nodes == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                nodes[index] = value;
            }
        }

        public Node(object tag) => Tag = tag;

        public Node(object tag, IEnumerable<Node> children) : this(tag) =>
            nodes = new List<Node>(children);

        public bool Contains(Node item) =>
            nodes != null && nodes.Contains(item);

        public bool ContainsInChild(Node item) {
            if(nodes == null)
                return false;
            if(nodes.Contains(item))
                return true;
            foreach(var child in nodes)
                if(child.ContainsInChild(item))
                    return true;
            return false;
        }

        public int IndexOf(Node item) =>
            nodes == null ? -1 : nodes.IndexOf(item);

        public void Add(Node item) {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if(ContainsInChild(item))
                throw new ArgumentException("Circular relationship detected.");
            if(nodes == null)
                nodes = new List<Node>();
            nodes.Add(item);
        }

        public void Insert(int index, Node item) {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if(ContainsInChild(item))
                throw new ArgumentException("Circular relationship detected.");
            if(nodes == null)
                nodes = new List<Node>();
            nodes.Insert(index, item);
        }

        public bool Remove(Node item) =>
            nodes != null && nodes.Remove(item);

        public void RemoveAt(int index) {
            if(nodes == null)
                throw new ArgumentOutOfRangeException(nameof(index));
            nodes.RemoveAt(index);
        }

        public void Clear() =>
            nodes?.Clear();

        public void CopyTo(Node[] array, int arrayIndex) =>
            nodes?.CopyTo(array, arrayIndex);

        public IEnumerator<Node> GetEnumerator() => nodes == null ?
            Enumerable.Empty<Node>().GetEnumerator() :
            nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        object ICloneable.Clone() =>
            new Node(Tag, nodes);

        public override string ToString() {
            var sb = new StringBuilder();
            ExpressionHelper.Serialize(this, sb);
            return sb.ToString();
        }

        public static Node Deserialize(string source) {
            if(string.IsNullOrWhiteSpace(source))
                return null;
            return ExpressionHelper.Deserizlize(source);
        }
    }
}