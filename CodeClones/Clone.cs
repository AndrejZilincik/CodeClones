using System.ComponentModel;
using System.IO;

namespace CodeClones
{
    public class Clone : INotifyPropertyChanged
    {
        public int StartLine1 { get; private set; }
        public int EndLine1 { get; private set; }
        
        public int StartLine2 { get; private set; }
        public int EndLine2 { get; private set; }

        private string _fileName1;
        public string FileName1
        {
            get
            {
                return Path.GetFileName(_fileName1);
            }
            set
            {
                _fileName1 = value;
                OnPropertyChanged("FileName1");
            }
        }
        private string _code1;
        public string Code1
        {
            get
            {
                return _code1;
            }
            set
            {
                _code1 = value;
                OnPropertyChanged("Code1");
            }
        }

        private string _fileName2;
        public string FileName2
        {
            get
            {
                return Path.GetFileName(_fileName2);
            }
            set
            {
                _fileName2 = value;
                OnPropertyChanged("FileName2");
            }
        }
        private string _code2;
        public string Code2
        {
            get
            {
                return _code2;
            }
            set
            {
                _code2 = value;
                OnPropertyChanged("Code2");
            }
        }
        
        // PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        public Clone(int startLine1, int endLine1, int startLine2, int endLine2)
        {
            this.StartLine1 = startLine1;
            this.EndLine1 = endLine1;
            
            this.StartLine2 = startLine2;
            this.EndLine2 = endLine2;
        }
    }
}