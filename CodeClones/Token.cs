using System.ComponentModel;

namespace CodeClones
{
    public enum TokenType { Keyword, Identifier, Symbol }

    public class Token : INotifyPropertyChanged
    {
        // Value/content of the token
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public TokenType Type { get; private set; }
        public int LineNumber { get; private set; }

        // PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Token(TokenType type, string val, int lineNumber)
        {
            this.Type = type;
            this.Value = val;
            this.LineNumber = lineNumber;
        }
    }
}