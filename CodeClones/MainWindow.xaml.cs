using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        // Check whether a file is loaded
        public bool IsFileLoaded
        {
            get
            {
                return FileList.Count >= 1;
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
        // Clone cache
        private CloneCache cloneCache = new CloneCache();
        // File content cache
        private Dictionary<string, IEnumerable<string>> fileContents = new Dictionary<string, IEnumerable<string>>();
        // Minimum clone size in lines
        private int _minLines = 5;
        public int MinLines
        {
            get
            {
                return _minLines;
            }
            set
            {
                if (value > 0)
                {
                    _minLines = value;
                    OnPropertyChanged("MinLines");
                }
            }
        }
        // Minimum clone size in tokens
        private int _minTokens = 10;
        public int MinTokens
        {
            get
            {
                return _minTokens;
            }
            set
            {
                if (value > 0)
                {
                    _minTokens = value;
                    OnPropertyChanged("MinTokens");
                }
            }
        }
        // Clone viewer textbox scrollbars
        ScrollViewer scroll1;
        ScrollViewer scroll2;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // Update IsFileLoaded when file list is modified
            FileList.CollectionChanged += (s, e) => { OnPropertyChanged("IsFileLoaded"); };

            // Bind clone viewer textbox scrollbars
            this.Loaded += (s, e) =>
            {
                scroll1 = (ScrollViewer)GetDescendantByType(Clones1, typeof(ScrollViewer));
                scroll2 = (ScrollViewer)GetDescendantByType(Clones2, typeof(ScrollViewer));
            };
        }
        
        // Setup and display open file dialog
        private IEnumerable<string> OpenFiles(bool multiSelect)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = multiSelect;
            ofd.Title = "Select a file to open...";
            ofd.DefaultExt = ".cpp";
            ofd.Filter = "C/C++ source files|*.cpp;*.hpp;*.c;*.h|All files|*.*";

            if (ofd.ShowDialog() == true)
            {
                return ofd.FileNames;
            }
            else
            {
                return null;
            }
        }

        // Compare token lists, create clone list
        private List<Clone> CompareTokenLists(TokenList tokens1, TokenList tokens2)
        {
            // Make sure files exist
            if (!File.Exists(tokens1.FileName) || !File.Exists(tokens2.FileName))
            {
                return new List<Clone>();
            }

            // Try to read clone list from cache
            List<Clone> clones = cloneCache.TryGet(tokens1.FileName, tokens2.FileName, MinLines, MinTokens);
            if (clones != null)
            {
                return clones;
            }

            // Get list of clones
            CloneFinder cloneFinder = new CloneFinder(tokens1, tokens2, MinLines, MinTokens);
            clones = cloneFinder.FindClones();

            // Get actual code segments
            foreach (Clone clone in clones)
            {
                // Calculate number of padding lines needed to make both clones the same length
                int length1 = clone.EndLine1 - clone.StartLine1;
                int length2 = clone.EndLine2 - clone.StartLine2;
                int pad1 = length2 > length1 ? length2 - length1 : 0;
                int pad2 = length1 > length2 ? length1 - length2 : 0;

                // Get relevant portions of source files, add padding lines
                clone.Code1 = string.Join(Environment.NewLine, fileContents[tokens1.FileName].Skip(clone.StartLine1 - 1).Take(length1 + 1));
                clone.Code2 = string.Join(Environment.NewLine, fileContents[tokens2.FileName].Skip(clone.StartLine2 - 1).Take(length2 + 1));
                clone.Code1 += string.Concat(Enumerable.Repeat(Environment.NewLine, pad1 + 1));
                clone.Code2 += string.Concat(Enumerable.Repeat(Environment.NewLine, pad2 + 1));
            }

            // Store clone list in cache
            cloneCache.AddEntry(tokens1.FileName, tokens2.FileName, MinLines, MinTokens, clones);

            return clones;
        }

        // Compare set of lists with each other
        private List<Clone> CompareManyTokenLists(IEnumerable<TokenList> tokenLists)
        {
            // Store contents of files being compared
            foreach (TokenList tokens in tokenLists)
            {
                fileContents.Add(tokens.FileName, File.ReadLines(tokens.FileName));
            }

            List<Clone> clones = new List<Clone>();

            // Compare each token list with all token lists after it
            foreach (TokenList tokens1 in tokenLists)
            {
                foreach(TokenList tokens2 in tokenLists.SkipWhile(t => !t.Tokens.SequenceEqual(tokens1.Tokens)))
                {
                    clones.AddRange(CompareTokenLists(tokens1, tokens2));
                }
            }

            // Clear file content cache
            fileContents.Clear();

            return clones;
        }

        #region UI Element Event Handlers
        // File 1 selector text box click event handler
        private void File1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Select file
            string fileName = OpenFiles(false)?.Single();
            if (fileName == null)
            {
                return;
            }

            // Add file to file list
            if (FileList.Count < 1)
            {
                FileList.Add(new TokenList(fileName));
            }
            else if (!FileList.Any(f => f.FileName == fileName))
            {
                FileList[0] = new TokenList(fileName);
            }
        }

        // File 2 selector text box click event handler
        private void File2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Select file
            string fileName = OpenFiles(false)?.Single();
            if (fileName == null)
            {
                return;
            }

            // Add file to file list
            if (FileList.Count < 2)
            {
                FileList.Add(new TokenList(fileName));
            }
            else if (!FileList.Any(f => f.FileName == fileName))
            {
                FileList[1] = new TokenList(fileName);
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
            MessageBox.Show("Code Clone Finder v0.1\n© 2016 Andrej Zilincik, et al.", "About");
        }

        // Find clones button click event handler
        private void FindClones_Click(object sender, RoutedEventArgs e)
        {
            TabBar.SelectedItem = ClonesTab;
        }

        // Add files to list of files to find clones in
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> fileNames = OpenFiles(true);

            // No files selected
            if (fileNames == null)
            {
                return;
            }

            // Add all selected files if not already in list
            foreach (string file in fileNames)
            {
                if (!FileList.Any(f => f.FileName == file))
                {
                    FileList.Add(new TokenList(file));
                }
            }
        }

        // Clear list of files to find clones in
        private void ClearFiles_Click(object sender, RoutedEventArgs e)
        {
            FileList.Clear();
        }

        // Synchronise scrolls in clone display tab
        private void CloneScroll1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scroll2.ScrollToVerticalOffset(e.VerticalOffset);
        }
        private void CloneScroll2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scroll1.ScrollToVerticalOffset(e.VerticalOffset);
        }
        
        // Switch tab event handler
        private void TabBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && (sender as TabControl).SelectedItem as TabItem == ClonesTab)
            {
                // Simulate go button click - Find clones and populate clone list
                CloneList = CompareManyTokenLists(FileList);
            }
        }

        // Update search settings - button click event handler
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Find clones
            CloneList = CompareManyTokenLists(FileList);
        }
        #endregion

        #region Generic helper methods
        // PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        // Needed to access list box scrolls
        private Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
        #endregion
    }
}
