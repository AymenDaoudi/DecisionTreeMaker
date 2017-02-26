using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DecisionTreeMaker.Model;
using FSharp.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GraphSharp.Controls;
using static DecisionTree;
using static Types;
using static CsvFileReader;

namespace DecisionTreeMaker.ViewModel
{
    public class PocGraphLayout : GraphLayout<Node, Branch, Tree> { }
    public class MainViewModel : ViewModelBase
    {
        #region Consts

        private const string OpenFileDialogFilter = "csv files (*.csv)|";
        private const string ClassHeader = "Class";
        #endregion

        #region Fields

        private string _dataSetFilePath;
        private string _graphLayoutType;
        private Tree _graphTree;
        private List<String> _graphLayoutTypes = new List<string>();
        private DataTable _dataSet;
        #endregion

        #region Properties

        public string DataSetFilePath
        {
            get { return _dataSetFilePath; }
            set
            {
                if (value == null) return;
                _dataSetFilePath = value;
                if (ConsumeFile())
                {
                    RaisePropertyChanged(nameof(DataSetFilePath));
                }
            }
        }
        public string GraphLayoutType
        {
            get { return _graphLayoutType; }
            set
            {
                _graphLayoutType = value; 
                RaisePropertyChanged(nameof(GraphLayoutType));
            }
        }
        public List<string> GraphLayoutTypes
        {
            get { return _graphLayoutTypes; }
            set
            {
                _graphLayoutTypes = value; 
                RaisePropertyChanged(nameof(GraphLayoutTypes));
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
        public DataTable DataSet
        {
            get { return _dataSet; }
            set
            {
                _dataSet = value; 
                RaisePropertyChanged(nameof(DataSet));
            }
        }

        #endregion

        public MainViewModel()
        {
            SetCommands();
        }

        #region Methods
        private Branch AddNewBranchToGraph(Node from, Node to, string branchContent, Tree graph)
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
                parent.Children.Select((node, i) => AddNewBranchToGraph(parent, node, tree.children.Value[i].branch.Value, graph)).ToList();
                return parent;
            }
        }

        private DataTable MakeDataSet(CsvFile data)
        {
            var dataTable = new DataTable("DataTable");
            dataTable.Columns.AddRange(getCsvFileHeaders(data).Select(header => new DataColumn(header, typeof(string))).ToArray());
            var t = getRows(data).ToList();
            t.Select(csvRow => dataTable.Rows.Add(csvRow.Cast<object>().ToArray())).ToList();
            return dataTable;
        }

        private void SetCommands()
        {
            Loaded = new RelayCommand(SetGraphLayoutTypes);
            OpenFileCommand = new RelayCommand(OpenDataSetFile);
        }

        private void SetGraphLayoutTypes()
        {
            GraphLayoutTypes.Add("BoundedFR");
            GraphLayoutTypes.Add("Circular");
            GraphLayoutTypes.Add("CompoundFDP");
            GraphLayoutTypes.Add("EfficientSugiyama");
            GraphLayoutTypes.Add("FR");
            GraphLayoutTypes.Add("ISOM");
            GraphLayoutTypes.Add("KK");
            GraphLayoutTypes.Add("LinLog");
            GraphLayoutTypes.Add("Tree");

            //Pick a default Layout Algorithm Type
            GraphLayoutType = "EfficientSugiyama";
        }

        private void OpenDataSetFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = OpenFileDialogFilter;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            DataSetFilePath = openFileDialog.FileName;
        }

        private CsvFile LoadDataSetFile(string dataSetFilePath)
        {
            CsvFile data = null;
            if (dataSetFilePath == null) return null;
            try
            {
                data = readCsvFile(dataSetFilePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Concat(exception.Message, " : The provided file is empty"),
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            return data;
        }

        private void PopulateDataSet(CsvFile data)
        {
            DataSet = MakeDataSet(data);
        }

        private void DrawTreeGraph(CsvFile data)
        {
            var excludedHeaders = new List<string> { ClassHeader };
            var treeHead = makeTree(data, excludedHeaders, null);
            var graph = new Tree(false);
            var tree = MakeTreeGraph(graph, treeHead.Value.Head);
            GraphTree = graph;
        }

        private bool ConsumeFile()
        {
            CsvFile csvData = LoadDataSetFile(DataSetFilePath);
            try
            {
                if (csvData == null) return false;
                PopulateDataSet(csvData);
                DrawTreeGraph(csvData);
            }
            catch (IOException ioException)
            {
                MessageBox.Show(string.Concat(ioException.Message, " : The provided file is empty"),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (InvalidDataException invalidDataException)
            {
                MessageBox.Show(string.Concat(invalidDataException.Message, " : The last header must have the name \"Class\""),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        #endregion

        #region Commands

        private RelayCommand _loaded;
        private RelayCommand _openFileCommand;

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
        public RelayCommand OpenFileCommand
        {
            get { return _openFileCommand; }
            set
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = value;
                    RaisePropertyChanged(nameof(OpenFileCommand));
                }
            }
        }

        #endregion
    }
}