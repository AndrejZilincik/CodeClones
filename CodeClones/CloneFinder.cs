using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeClones
{
    public class CloneFinder
    {
        // Minimum length in lines to be considered a clone
        static readonly int MinLines = 3;

        // Minimum number of tokens to be considered a clone
        static readonly int MinTokens = 5;

        private List<Token> TokenList1;
        private List<Token> TokenList2;

        Dictionary<string, string> Identifiers = new Dictionary<string, string>();

        public CloneFinder(List<Token> tokenList1, List<Token> tokenList2)
        {
            this.TokenList1 = tokenList1;
            this.TokenList2 = tokenList2;
        }

        // Find clones in a pair of token lists
        public List<Clone> FindClones()
        {
            List<Clone> clones = new List<Clone>();

            // Get indexes of all tokens at the start of a line
            List<int> lineStartTokens1 = GetLineStartTokens(TokenList1);
            List<int> lineStartTokens2 = GetLineStartTokens(TokenList2);

            // Starting at these tokens, compare tokens in both lists until they do not match
            for (int i = 0; i < lineStartTokens1.Count - MinLines + 1; i++)
            {
                for (int j = 0; j < lineStartTokens2.Count - MinLines + 1; j++)
                {
                    int initIndex1 = lineStartTokens1[i];
                    int initIndex2 = lineStartTokens2[j];
                    int index1 = lineStartTokens1[i];
                    int index2 = lineStartTokens2[j];
                    
                    // Compare tokens at beginning of line
                    if (CompareTokens(TokenList1[index1], TokenList2[index2]))
                    {
                        // Keep comparing tokens until they no longer match
                        while (index1 + 1 < TokenList1.Count && index2 + 1 < TokenList2.Count && CompareTokens(TokenList1[index1 + 1], TokenList2[index2 + 1]))
                        {
                            index1++;
                            index2++;
                        }

                        // Empty stored identifiers
                        Identifiers.Clear();

                        // If clone is long enough, add it to the clone list
                        if (TokenList1[index1].LineNumber - TokenList1[initIndex1].LineNumber >= MinLines &&
                            TokenList2[index2].LineNumber - TokenList2[initIndex2].LineNumber >= MinLines &&
                            index1 - initIndex1 >= MinTokens)
                        {
                            clones.Add(new Clone(TokenList1[initIndex1].LineNumber, TokenList1[index1].LineNumber, TokenList2[initIndex2].LineNumber, TokenList2[index2].LineNumber));

                            //// Advance search index past end of clone
                            //while (i < lineStartTokens1.Count && lineStartTokens1[i] < index1 + 1)
                            //{
                            //    i++;
                            //}
                        }
                    }
                }
            }

            return clones;
        }

        // Given a token list, return first token in each line
        private List<int> GetLineStartTokens(List<Token> tokenList)
        {
            List<int> lineStartTokens = new List<int>();
            
            int line = 0;
            for (int i = 0; i < tokenList.Count; i++)
            {
                if (tokenList[i].LineNumber != line)
                {
                    // If token is on a new line, add it
                    line = tokenList[i].LineNumber;
                    lineStartTokens.Add(i);
                }
            }

            return lineStartTokens;
        }

        // Compare tokens
        private bool CompareTokens(Token token1, Token token2)
        {
            if (token1.Type != token2.Type)
            {
                // Tokens are not the same if their type does not match
                return false;
            }
            else if (token1.Value == "include")
            {
                // Ignore clones containing include statements
                return false;
            }
            else if (token1.Type == TokenType.Identifier)
            {
                if (Identifiers.ContainsKey(token1.Value))
                {
                    // Lookup identifier in dictionary
                    return token2.Value == Identifiers[token1.Value];
                }
                else
                {
                    // Add identifier to dictionary
                    Identifiers.Add(token1.Value, token2.Value);
                    return true;
                }
            }
            else
            {
                // Compare token values
                return token1.Value == token2.Value;
            }
        }
    }
}