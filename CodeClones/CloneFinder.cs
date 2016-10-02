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
                    // If comparing a file to itself, ignore clones that start at the same point in the file, and remove duplicates
                    if (FileName1 == FileName2 && line1 >= line2)
                    {
                        continue;
                    }

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
                                int endLine1;
                                int endLine2;

                                // Last matching token is at end of line
                                if (index1 + 1 >= TokenList1.Count || TokenList1[index1].LineNumber != TokenList1[index1 + 1].LineNumber)
                                {
                                    endLine1 = TokenList1[index1].LineNumber;
                                }
                                else
                                {
                                    endLine1 = TokenList1[index1].LineNumber - 1;
                                }
                                if (index2 + 1 >= TokenList2.Count || TokenList2[index2].LineNumber != TokenList2[index2 + 1].LineNumber)
                                {
                                    endLine2 = TokenList2[index2].LineNumber;
                                }
                                else
                                {
                                    endLine2 = TokenList2[index2].LineNumber - 1;
                                }

                                // Add clone to clone list
                                clones.Add(new Clone(FileName1, TokenList1[initIndex1].LineNumber, endLine1, FileName2, TokenList2[initIndex2].LineNumber, endLine2));
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
            // Tokens are not the same if their type does not match
            if (token1.Type != token2.Type)
            {
                return false;
            }
            // Ignore clones containing include statements
            else if (token1.Value == "include")
            {
                return false;
            }
            // Ignore clones containing include statements
            else if (token1.Value == "define")
            {
                return false;
            }
            // Compare identifiers
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
            // Compare non-identifier tokens
            else
            {
                return token1.Value == token2.Value;
            }
        }
    }
}