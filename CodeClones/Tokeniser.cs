using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeClones
{
    public class Tokeniser
    {
        List<Token> tokenList = new List<Token>();
        string Text;

        public Tokeniser(string fileName)
        {
            // Read all text from file into a string
            Text = File.ReadAllText(fileName);
        }

        public List<Token> GetTokens()
        {
            string temp = string.Empty;
            foreach (char c in Text)
            {
                if (char.IsLetterOrDigit(c))
                {
                    // Add letter/digit to current token
                    temp += c;
                }
                else
                {
                    if (temp.Length > 0)
                    {
                        // Add previous token to token list
                        tokenList.Add(new Token(temp));
                        temp = string.Empty;
                    }
                    if (!char.IsWhiteSpace(c))
                    {
                        // Add symbol to token list
                        tokenList.Add(new Token(c.ToString()));
                    }
                }
            }

            if (temp.Length > 0)
            {
                // Add last token to token list
                tokenList.Add(new Token(temp));
            }

            return tokenList;
        }
    }
}