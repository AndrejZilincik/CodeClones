using System.Collections.Generic;

namespace CodeClones
{
    public class TokenList
    {
        public string FileName { get; private set; }
        public List<Token> Tokens { get; private set; }

        public TokenList (string fileName)
        {
            this.FileName = fileName;
            this.Tokens = new Tokeniser(fileName).GetTokens();
        }
    }
}
