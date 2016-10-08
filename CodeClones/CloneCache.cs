using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CodeClones
{
    class CloneCache
    {
        Dictionary<string, List<Clone>> table = new Dictionary<string, List<Clone>>();

        // Clear table
        public void Clear()
        {
            table.Clear();
        }

        // Add entry to table
        public void AddEntry(string fileName1, string fileName2, CloneSearchParameters searchParameters, List<Clone> clones)
        {
            table.Add(GetHash(fileName1, fileName2, searchParameters), clones);
        }

        // Check table for entry matching given filenames
        public List<Clone> TryGet(string fileName1, string fileName2, CloneSearchParameters searchParameters)
        {
            string key = GetHash(fileName1, fileName2, searchParameters);
            if (table.ContainsKey(key))
            {
                return table[key];
            }
            else
            {
                return null;
            }
        }

        // Read hashtable from a file
        public void ReadFromFile(string fileName)
        {
            // TODO: Implement reading CloneCache from file
        }

        // Write hashtable to a file
        public void WriteToFile(string fileName)
        {
            // TODO: Implement writing CloneCache to file
        }

        // Get hash representing pair of files
        private string GetHash(string fileName1, string fileName2, CloneSearchParameters searchParameters)
        {
            byte[] hash1;
            byte[] hash2;
            byte[] hash;
            
            using (MD5 md5 = MD5.Create())
            {
                // Get hash of file 1 contents
                using (FileStream fs = File.OpenRead(fileName1))
                {
                    hash1 = md5.ComputeHash(fs);
                }
                // Get hash of file 2 contents
                using (FileStream fs = File.OpenRead(fileName2))
                {
                    hash2 = md5.ComputeHash(fs);
                }
                // Combine hashes and min clone length data into single hash
                byte[] comb = hash1.Concat(hash2)
                    .Concat(BitConverter.GetBytes(searchParameters.MinLines))
                    .Concat(BitConverter.GetBytes(searchParameters.MinTokens))
                    .Concat(BitConverter.GetBytes(searchParameters.PercentMatch))
                    .ToArray();
                hash = md5.ComputeHash(comb);
            }

            // Convert hash to hex
            return string.Concat(hash.Select(b => b.ToString("X2")));
        }
    }
}
