using System.ComponentModel;

namespace CodeClones
{
    public enum TokenType { Keyword, Identifier, Symbol }

    public class Token
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
            }
        }

        public TokenType Type { get; private set; }
        public int LineNumber { get; private set; }

        public Token(TokenType type, string val, int lineNumber)
        {
            this.Type = type;
            this.Value = val;
            this.LineNumber = lineNumber;
        }
    }
}