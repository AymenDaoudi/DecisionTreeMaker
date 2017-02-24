using System;
using System.Collections.Generic;
using System.Linq;
using DecisionTreeMaker.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static DecisionTree;
using static Types;

namespace DecisionTreeMaker.ViewModel
{
    public class PocGraphLayout : GraphSharp.Controls.GraphLayout<Node, Branch, Tree> { }
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private string _layoutAlgorithmType;
        private Tree _graphTree;
        private List<String> _layoutAlgorithmTypes = new List<string>();
        private FSharpOption<FSharpList<node>> treeHead;
        #endregion

        #region Properties
        public string LayoutAlgorithmType
        {
            get { return _layoutAlgorithmType; }
            set
            {
                _layoutAlgorithmType = value; 
                RaisePropertyChanged(nameof(LayoutAlgorithmType));
            }
        }
        public List<string> LayoutAlgorithmTypes
        {
            get { return _layoutAlgorithmTypes; }
            set
            {
                _layoutAlgorithmTypes = value; 
                RaisePropertyChanged(nameof(LayoutAlgorithmTypes));
            }
        }
        public Tree GraphTree
        {
            get { return _graphTree; }
            set
            {
                _graphTree = value; 
                RaisePropertyChanged(nameof(GraphTree));
            }
        }
        
        #endregion

        public MainViewModel()
        {
            SetCommands();
            var data = readCsvFile(@"C:\Users\Aymen\Desktop\TrainingSet.csv");
            var excludedHeaders = new List<string> { "Class" };
            treeHead = makeTree(data, excludedHeaders, null);
            var graph = new Tree(false);
            var tree = MakeTreeGraph(graph, treeHead.Value.Head);
            GraphTree = graph;
        }


        #region Methods
        private Branch AddNewGraphBranch(Node from, Node to, string branchContent, Tree graph)
        {
            var newBranch = new Branch(branchContent, from, to);

            graph.AddEdge(newBranch);
            return newBranch;
        }

        private Node MakeTreeGraph(Tree graph, node tree)
        {
            if (tree.isLeaf)
            {
                var leaf = new Node(tree.content, tree.isLeaf, null);
                graph.AddVertex(leaf);
                return leaf;
            }
            else
            {
                var parent = new Node(tree.content, tree.isLeaf, tree.children.Value.Select(node => MakeTreeGraph(graph,node)).ToList());
                graph.AddVertex(parent);
                graph.AddVertexRange(parent.Children);
                parent.Children.Select((node, i) => AddNewGraphBranch(parent, node, tree.children.Value[i].branch.Value, graph)).ToList();
                return parent;
            }
        }


        public void SetCommands()
        {
            Loaded = new RelayCommand(() =>
            {
                LayoutAlgorithmTypes.Add("BoundedFR");
                LayoutAlgorithmTypes.Add("Circular");
                LayoutAlgorithmTypes.Add("CompoundFDP");
                LayoutAlgorithmTypes.Add("EfficientSugiyama");
                LayoutAlgorithmTypes.Add("FR");
                LayoutAlgorithmTypes.Add("ISOM");
                LayoutAlgorithmTypes.Add("KK");
                LayoutAlgorithmTypes.Add("LinLog");
                LayoutAlgorithmTypes.Add("Tree");

                //Pick a default Layout Algorithm Type
                LayoutAlgorithmType = "EfficientSugiyama";
            });
        }
        #endregion

        #region Commands

        private RelayCommand _loaded;

        public RelayCommand Loaded
        {
            get
            {
                return _loaded;
            }
            set
            {
                if (_loaded == null)
                {
                    _loaded = value;
                    RaisePropertyChanged(nameof(Loaded));
                }   
            }
        }

        #endregion
    }
}