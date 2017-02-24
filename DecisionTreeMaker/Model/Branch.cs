using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace DecisionTreeMaker.Model
{
    [DebuggerDisplay("{Source.Content} -> {Target.Content}")]
    public class Branch : Edge<Node>
    {

        #region Properties

        public string Content { get; private set; }
        #endregion

        public Branch(string content, Node source, Node target): base(source, target)
        {
            Content = content;
        }

    }
}
