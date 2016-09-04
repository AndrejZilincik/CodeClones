using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace CodeClones
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // List of files to find clones in
        private ObservableCollection<TokenList> _fileList = new ObservableCollection<TokenList>();
        public ObservableCollection<TokenList> FileList
        {
            get
            {
                return _fileList;
            }
            set
            {
                _fileList = value;
                OnPropertyChanged("FileList");
            }
        }

        // Check whether two files are loaded
        public bool AreTwoFilesLoaded
        {
            get
            {
                return FileList.Count >= 2;
            }
        }
        
        // List of code clones
        private List<Clone> _cloneList = new List<Clone>();
        public List<Clone> CloneList
        {
            get
            {
                return _cloneList;
            }
            set
            {
                _cloneList = value;
                OnPropertyChanged("CloneList");
            }
        }
        
        // PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            FileList.CollectionChanged += (s, e) => { OnPropertyChanged("AreTwoFilesLoaded"); };
        }

        // Setup and display open file dialog
        private string OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a file to open...";
            ofd.DefaultExt = ".cpp";
            ofd.Filter = "C/C++ source files|*.cpp;*.hpp;*.c;*.h|All files|*.*";

            if (ofd.ShowDialog() == true)
            {
                return ofd.FileName;
            }
            else
            {
                return null;
            }
        }

        // Compare token lists, create clone list
        private List<Clone> CompareTokenLists(TokenList tokens1, TokenList tokens2)
        {
            // Read source files, separated into individual lines
            IEnumerable<string> lines1 = File.ReadLines(tokens1.FileName);
            IEnumerable<string> lines2 = File.ReadLines(tokens2.FileName);

            // Get list of clones
            CloneFinder cloneFinder = new CloneFinder(tokens1, tokens2);
            List<Clone> clones = cloneFinder.FindClones();

            // Get relevant portions of source files
            foreach (Clone clone in clones)
            {
                clone.Code1 = string.Join(Environment.NewLine, lines1.Skip(clone.StartLine1 - 1).Take(clone.EndLine1 - clone.StartLine1 + 1));
                clone.Code2 = string.Join(Environment.NewLine, lines2.Skip(clone.StartLine2 - 1).Take(clone.EndLine2 - clone.StartLine2 + 1));
            }

            return clones;
        }

        // Compare two or more token lists with each other
        private List<Clone> CompareManyTokenLists(IEnumerable<TokenList> tokenLists)
        {
            List<Clone> clones = new List<Clone>();

            // Compare each token list with all token lists after it
            foreach (TokenList tokens1 in tokenLists)
            {
                foreach(TokenList tokens2 in tokenLists.SkipWhile(t => !t.Tokens.SequenceEqual(tokens1.Tokens)).Skip(1))
                {
                    clones.AddRange(CompareTokenLists(tokens1, tokens2));
                }
            }

            return clones;
        }

        #region UI Element Event Handlers
        // File 1 selector text box click event handler
        private void File1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Select file
            string fileName = OpenFile();
            if (fileName == null)
            {
                return;
            }

            // Add file to file list
            if (!FileList.Any(f => f.FileName == fileName))
            {
                FileList.Insert(0, new TokenList(fileName));
            }
        }

        // File 2 selector text box click event handler
        private void File2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FileList.Count < 1)
            {
                return;
            }

            // Select file
            string fileName = OpenFile();
            if (fileName == null)
            {
                return;
            }

            // Add file to file list
            if (!FileList.Any(f => f.FileName == fileName))
            {
                FileList.Insert(1, new TokenList(fileName));
            }
        }

        // Exit menu item click event handler
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // About menu item click event handler
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Code Clone Finder v0.1\n© 2016 Andrej Zilincik", "About");
        }

        // Find clones button click event handler
        private void FindClones_Click(object sender, RoutedEventArgs e)
        {
            TabBar.SelectedItem = ClonesTab;
        }

        // Switch tab event handler
        private void TabBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && (sender as TabControl).SelectedItem as TabItem == ClonesTab)
            {
                // Find clones and populate clone list
                CloneList = CompareManyTokenLists(FileList);
            }
        }

        // Add files to list of files to find clones in
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            // Display open file dialog
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "Select files to add...";
            ofd.DefaultExt = ".cpp";
            ofd.Filter = "C/C++ source files|*.cpp;*.hpp;*.c;*.h|All files|*.*";

            // Add selected files
            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!FileList.Any(f => f.FileName == file))
                    {
                        FileList.Add(new TokenList(file));
                    }
                }
            }
        }

        // Clear list of files to find clones in
        private void ClearFiles_Click(object sender, RoutedEventArgs e)
        {
            FileList.Clear();
        }
        #endregion
    }
}
