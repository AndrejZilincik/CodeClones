using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace CodeClones
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Path to file 1
        private string _fileName1;
        public string FileName1
        {
            get
            {
                return _fileName1;
            }
            set
            {
                _fileName1 = value;
                OnPropertyChanged("FileName1");
            }
        }

        // Path to file 2
        private string _fileName2;
        public string FileName2
        {
            get
            {
                return _fileName2;
            }
            set
            {
                _fileName2 = value;
                OnPropertyChanged("FileName2");
            }
        }

        // List of tokens in file 1
        private List<Token> _file1Tokens = new List<Token>();
        public List<Token> File1Tokens
        {
            get
            {
                return _file1Tokens;
            }
            set
            {
                _file1Tokens = value;
                OnPropertyChanged("File1Tokens");
                OnPropertyChanged("AreTwoFilesLoaded");
            }
        }

        // List of tokens in file 2
        private List<Token> _file2Tokens = new List<Token>();
        public List<Token> File2Tokens
        {
            get
            {
                return _file2Tokens;
            }
            set
            {
                _file2Tokens = value;
                OnPropertyChanged("File2Tokens");
                OnPropertyChanged("AreTwoFilesLoaded");
            }
        }

        // Check whether two files are loaded
        public bool AreTwoFilesLoaded
        {
            get
            {
                return (FileName1 != null && FileName2 != null);
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
        private void CompareFiles()
        {
            // Clear clone lists
            CloneList.Clear();

            // Read source files, separate into individual lines
            List<string> lines1 = File.ReadLines(FileName1).ToList();
            List<string> lines2 = File.ReadLines(FileName2).ToList();

            // Get list of clones
            CloneFinder cloneFinder = new CloneFinder(File1Tokens, File2Tokens);
            List<Clone> clones = cloneFinder.FindClones();

            // Combine relevant portions of source files
            foreach (Clone clone in clones)
            {
                string code1 = $"{Path.GetFileName(FileName1)}, lines {clone.StartLine1}-{clone.EndLine1}:" + Environment.NewLine + Environment.NewLine;
                code1 += string.Join(Environment.NewLine, lines1.Skip(clone.StartLine1 - 1).Take(clone.EndLine1 - clone.StartLine1 + 1));
                clone.Code1 = code1;

                string code2 = $"{Path.GetFileName(FileName2)}, lines {clone.StartLine2}-{clone.EndLine2}:" + Environment.NewLine + Environment.NewLine;
                code2 += string.Join(Environment.NewLine, lines2.Skip(clone.StartLine2 - 1).Take(clone.EndLine2 - clone.StartLine2 + 1));
                clone.Code2 = code2;
            }

            CloneList = clones;
        }

        // File 1 selector text box click event handler
        private void File1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            FileName1 = OpenFile() ?? FileName1;
            if (FileName1 != null)
            {
                Tokeniser tokeniser = new Tokeniser(FileName1);
                File1Tokens = tokeniser.GetTokens();
            }
        }

        // File 2 selector text box click event handler
        private void File2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            FileName2 = OpenFile() ?? FileName2;
            if (FileName2 != null)
            {
                Tokeniser tokeniser = new Tokeniser(FileName2);
                File2Tokens = tokeniser.GetTokens();
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
            TabBar.SelectedIndex = 1;
        }

        // Clones tab got focus event handler
        private void ClonesTab_GotFocus(object sender, RoutedEventArgs e)
        {
            CompareFiles();
        }
    }
}
