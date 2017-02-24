using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTreeMaker.Model
{
    [DebuggerDisplay("{Content}-{IsLeaf}")]
    public class Node
    {
        #region Properties
        public string Content { get; private set; }
        public bool IsLeaf { get; private set; }
        public List<Node> Children { get; private set; }
        #endregion

        public Node(string content, bool isLeaf, List<Node> children)
        {
            Content = content;
            IsLeaf = isLeaf;
            Children = children;
        }

        #region Methods
        public override string ToString() => Content;
        #endregion






    }
}
