using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
