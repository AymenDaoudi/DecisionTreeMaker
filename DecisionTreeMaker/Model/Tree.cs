using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace DecisionTreeMaker.Model
{
    public class Tree : BidirectionalGraph<Node,Branch>
    {
        public Tree() { }

        public Tree(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        public Tree(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }
    }
}
