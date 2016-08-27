using System.ComponentModel;

namespace CodeClones
{
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

        // PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Token(string val)
        {
            this.Value = val;
        }
    }
}