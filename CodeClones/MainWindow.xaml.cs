using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
    }
}
