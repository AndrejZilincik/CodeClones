using System;
using System.Collections;

namespace CodeClones
{
    public class CloneSearchParameters
    {
        public int MinLines { get; set; }
        public int MinTokens { get; set; }
        public double MinPercentMatch { get; set; }

        public CloneSearchParameters (int minLines, int minTokens, double percentMatch)
        {
            MinLines = minLines;
            MinTokens = minTokens;
            MinPercentMatch = percentMatch;
        }
    }
}
