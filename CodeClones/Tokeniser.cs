﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeClones
{
    public class Tokeniser
    {
        public static string[] KEYWORDS = { "alignas", "alignof", "and", "and_eq", "asm", "atomic_cancel", "atomic_commit", "atomic_noexcept", "auto", "bitand", "bitor", "bool", "break", "case", "catch", "char", "char16_t", "char32_t", "class", "compl", "concept", "const", "constexpr ", "const_cast", "continue", "decltype", "default", "delete", "do", "double", "dynamic_cast", "else", "enum", "explicit", "export", "extern", "false", "float", "for", "friend", "goto", "if", "inline", "int", "import", "long", "module", "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr", "operator", "or", "or_eq", "private", "protected", "public", "register", "reinterpret_cast", "requires", "return", "short", "signed", "sizeof", "static", "static_assert", "static_cast", "struct", "switch", "synchronized", "template", "this", "thread_local ", "throw", "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual", "void", "volatile", "wchar_t", "while", "xor", "xor_eq" };
        
        List<Token> tokenList = new List<Token>();
        string Text;

        public Tokeniser(string fileName)
        {
            // Read all text from file into a string
            Text = File.ReadAllText(fileName);

            // Remove comments from text
            Text = RemoveComments(Text);
        }

        public List<Token> GetTokens()
        {
            int lineNumber = 1;
            string str = string.Empty;
            foreach (char c in Text)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    // Part of identifier/keyword - add to current token
                    str += c;
                }
                else
                {
                    // Add previous token to token list
                    AddStringToken(str, lineNumber);

                    str = string.Empty;
                    if (!char.IsWhiteSpace(c))
                    {
                        // Add symbol to token list
                        tokenList.Add(new Token(TokenType.Symbol, c.ToString(), lineNumber));
                    }
                    else if (c == '\n')
                    {
                        lineNumber++;
                    }
                }
            }

            // Add last token
            AddStringToken(str, lineNumber);
            return tokenList;
        }

        private void AddStringToken(string str, int lineNumber)
        {
            if (str.Length > 0)
            {
                // Add token to token list
                TokenType type = char.IsDigit(str[0]) ? TokenType.Literal : KEYWORDS.Contains(str) ? TokenType.Keyword : TokenType.Identifier;
                tokenList.Add(new Token(type, str, lineNumber));
            }
        }
        
        // Remove C-style single line and multiline comments
        private string RemoveComments(string text)
        {
            // TODO: Ignore comment symbols inside comments and literal strings
            
            // Remove single line comments
            int pos = text.IndexOf("//");
            int end;
            while (pos > -1)
            {
                end = text.IndexOf('\n', pos);
                if (end == -1)
                {
                    text = text.Remove(pos);
                    break;
                }
                text = text.Remove(pos, end - pos);
                pos = text.IndexOf("//", pos);
            }

            // Remove multiline comments
            pos = text.IndexOf("/*");
            while (pos > -1)
            {
                end = text.IndexOf("*/", pos);
                if (end == -1)
                {
                    break;
                }
                text = text.Remove(pos, end - pos);
                pos = text.IndexOf("/*", pos);
            }

            return text;
        }
    }
}