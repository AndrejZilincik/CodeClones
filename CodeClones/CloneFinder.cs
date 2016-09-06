using System.Collections.Generic;
using System.Linq;

namespace CodeClones
{
    public class CloneFinder
    {
        // Minimum length in lines to be considered a clone
        private int MinLines = 3;

        // Minimum number of tokens to be considered a clone
        private int MinTokens = 5;

        // Filenames of the files being searched
        private string FileName1;
        private string FileName2;

        // Token lists to find clones in
        private List<Token> TokenList1;
        private List<Token> TokenList2;

        // Stores previously used identifier names
        Dictionary<string, string> Identifiers = new Dictionary<string, string>();

        public CloneFinder(TokenList tokenList1, TokenList tokenList2, int minLines, int minTokens)
        {
            this.FileName1 = tokenList1.FileName;
            this.TokenList1 = tokenList1.Tokens;

            this.FileName2 = tokenList2.FileName;
            this.TokenList2 = tokenList2.Tokens;

            this.MinLines = minLines;
            this.MinTokens = minTokens;
        }

        // Find clones in a pair of token lists
        public List<Clone> FindClones()
        {
            List<Clone> clones = new List<Clone>();

            // Get indexes of all tokens at the start of a line
            List<int> lineStartTokens1 = GetLineStartTokens(TokenList1);
            List<int> lineStartTokens2 = GetLineStartTokens(TokenList2);

            // Starting at each pair of these tokens, compare tokens in both lists until tokens do not match
            for (int line1 = 0; line1 + MinLines - 1 < lineStartTokens1.Count; line1++)
            {
                for (int line2 = 0; line2 + MinLines - 1 < lineStartTokens2.Count; line2++)
                {
                    int initIndex1 = lineStartTokens1[line1];
                    int initIndex2 = lineStartTokens2[line2];
                    int index1 = lineStartTokens1[line1];
                    int index2 = lineStartTokens2[line2];

                    // Compare tokens at beginning of line
                    if (CompareTokens(TokenList1[index1], TokenList2[index2]))
                    {
                        // Check if clone overlaps with another clone
                        if (!clones.Any(c => c.StartLine1 <= TokenList1[index1].LineNumber && c.EndLine1 >= TokenList1[index1].LineNumber &&
                                             c.StartLine2 <= TokenList2[index2].LineNumber && c.EndLine2 >= TokenList2[index2].LineNumber))
                        {

                            // Keep comparing tokens until they no longer match
                            while (index1 + 1 < TokenList1.Count && index2 + 1 < TokenList2.Count && CompareTokens(TokenList1[index1 + 1], TokenList2[index2 + 1]))
                            {
                                // Move to next token
                                index1++;
                                index2++;
                            }

                            // Empty list of stored identifiers
                            Identifiers.Clear();

                            // If clone is long enough, add it to the clone list
                            if ((index1 + 1 >= TokenList1.Count || (line1 + MinLines < lineStartTokens1.Count && index1 >= lineStartTokens1[line1 + MinLines])) &&
                                (index2 + 1 >= TokenList2.Count || (line2 + MinLines < lineStartTokens2.Count && index2 >= lineStartTokens2[line2 + MinLines])) &&
                                index1 - initIndex1 >= MinTokens)
                            {
                                clones.Add(new Clone(FileName1, TokenList1[initIndex1].LineNumber, TokenList1[index1].LineNumber, FileName2, TokenList2[initIndex2].LineNumber, TokenList2[index2].LineNumber));
                            }
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