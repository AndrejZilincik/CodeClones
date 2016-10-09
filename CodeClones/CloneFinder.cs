using System.Collections.Generic;
using System.Linq;

namespace CodeClones
{
    // Finds clones by comparing a pair of token lists
    public class CloneFinder
    {
        // Minimum length in lines to be considered a clone
        private int MinLines;
        // Minimum number of tokens to be considered a clone
        private int MinTokens;
        // Percentage of tokens that need to match to be considered a clone
        private double MatchThreshold;
        // Ignore clones containing include keyword
        private bool IgnoreInclude = true;
        // Ignore clones containing define keyword
        private bool IgnoreDefine = true;
        // Ignore clones containing typedef keyword
        private bool IgnoreTypedef = true;

        // Filenames of the files being searched
        private string FileName1;
        private string FileName2;

        // Token lists to find clones in
        private List<Token> TokenList1;
        private List<Token> TokenList2;

        // List of found clones
        List<Clone> Clones;

        // Stores previously used identifier names
        Dictionary<string, string> Identifiers = new Dictionary<string, string>();
        
        public CloneFinder(TokenList tokenList1, TokenList tokenList2, CloneSearchParameters searchParameters)
        {
            this.FileName1 = tokenList1.FileName;
            this.TokenList1 = tokenList1.Tokens;

            this.FileName2 = tokenList2.FileName;
            this.TokenList2 = tokenList2.Tokens;

            this.MinLines = searchParameters.MinLines;
            this.MinTokens = searchParameters.MinTokens;
            this.MatchThreshold = searchParameters.MinPercentMatch / 100;
        }

        // Find clones in the token lists
        public List<Clone> FindClones()
        {
            Clones = new List<Clone>();

            // Get token from each list
            foreach (int index1 in GetLineStartTokens(TokenList1))
            {
                foreach (int index2 in GetLineStartTokens(TokenList2))
                {
                    // If comparing a file to itself, ignore clones that start at the same point in the file and remove duplicates
                    if (FileName1 == FileName2 && index1 >= index2)
                    {
                        continue;
                    }

                    // Ignore clones that overlap with another clone
                    if (OverlapsWithClone(index1, index2))
                    {
                        continue;
                    }

                    // Check for clones starting at that position
                    Clone clone = FindCloneAt(index1, index2);
                    if (clone != null)
                    {
                        Clones.Add(clone);
                    }
                }
            }

            return Clones;
        }

        // Check if there is already a clone which includes the specified tokens
        private bool OverlapsWithClone(int index1, int index2)
        {
            return Clones.Any(c => c.StartLine1 <= TokenList1[index1].LineNumber && c.EndLine1 >= TokenList1[index1].LineNumber &&
                                   c.StartLine2 <= TokenList2[index2].LineNumber && c.EndLine2 >= TokenList2[index2].LineNumber);
        }

        // Try to find clone starting at specified indices
        public Clone FindCloneAt(int initIndex1, int initIndex2)
        {
            // Compare tokens at starting indices
            TokenComparisonResult comp = CompareTokens(TokenList1[initIndex1], TokenList2[initIndex2]);
            if (comp != TokenComparisonResult.Matching)
            {
                return null;
            }
            double matches = 1;

            // Initialise token pointers
            int index1 = initIndex1 + 1;
            int index2 = initIndex2 + 1;
            int lastMatching1 = initIndex1;
            int lastMatching2 = initIndex2;

            // Keep comparing tokens until the percentage of matching tokens is below the specified threshold or there are no tokens left
            while (matches / (index1 - initIndex1) >= MatchThreshold && index1 + 1 < TokenList1.Count && index2 + 1 < TokenList2.Count)
            {
                // Move to next tokens
                index1++;
                index2++;

                // Compare tokens
                comp = CompareTokens(TokenList1[index1], TokenList2[index2]);
                if (comp == TokenComparisonResult.Ignore)
                {
                    // Stop comparing
                    break;
                }
                else if (comp == TokenComparisonResult.Matching)
                {
                    // Tokens match
                    matches++;
                    lastMatching1 = index1;
                    lastMatching2 = index2;
                }
            }

            // Empty list of stored identifiers
            Identifiers.Clear();

            // If clone is long enough, add it to the clone list
            if (LongEnough(initIndex1, index1, initIndex2, index2))
            {
                int matchPercentage = (int)(100 * matches / (lastMatching1 - initIndex1));
                return new Clone(FileName1, TokenList1[initIndex1].LineNumber, TokenList1[lastMatching1].LineNumber, FileName2, TokenList2[initIndex2].LineNumber, TokenList2[lastMatching2].LineNumber, matchPercentage);
            }
            else
            {
                return null;
            }
        }

        // Check if clone meets the minimum length requirements
        private bool LongEnough(int initIndex1, int index1, int initIndex2, int index2)
        {
            return index1 - initIndex1 >= MinTokens && index2 - initIndex2 >= MinTokens &&
                TokenList1[index1].LineNumber - TokenList1[initIndex1].LineNumber > MinLines &&
                TokenList2[index2].LineNumber - TokenList2[initIndex2].LineNumber > MinLines;
        }

        // Given a token list, return a list containing the first token in each line
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

        private enum TokenComparisonResult { Ignore, Different, Matching }

        // Compare tokens
        private TokenComparisonResult CompareTokens(Token token1, Token token2)
        {
            // Tokens are not the same if their type does not match
            if (token1.Type != token2.Type)
            {
                return TokenComparisonResult.Different;
            }
            // Ignore clones containing include keyword
            else if (IgnoreInclude && token1.Value == "include")
            {
                return TokenComparisonResult.Ignore;
            }
            // Ignore clones containing define keyword
            else if (IgnoreDefine && token1.Value == "define")
            {
                return TokenComparisonResult.Ignore;
            }
            // Ignore clones containing typedef keyword
            else if (IgnoreTypedef && token1.Value == "typedef")
            {
                return TokenComparisonResult.Ignore;
            }
            // Compare identifiers
            else if (token1.Type == TokenType.Identifier)
            {
                if (Identifiers.ContainsKey(token1.Value))
                {
                    // Lookup identifier in dictionary
                    return token2.Value == Identifiers[token1.Value] ? TokenComparisonResult.Matching : TokenComparisonResult.Different;
                }
                else
                {
                    // Add identifier to dictionary
                    Identifiers.Add(token1.Value, token2.Value);
                    return TokenComparisonResult.Matching;
                }
            }
            // Compare non-identifier tokens
            else
            {
                return token1.Value == token2.Value ? TokenComparisonResult.Matching : TokenComparisonResult.Different;
            }
        }
    }
}