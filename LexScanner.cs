using System;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq;

namespace compilador
{
    class LexScanner
    {

        public LexScanner(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    this.content = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                
            }

            this.content = System.IO.File.ReadAllText(path);
            
        }
        
        private string content { get; set; }
        private  int pos { get; set; }
        private int state { get; set; }
        
        private bool isLetter(char c)
        {
            return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
        }

        private bool isDigit(char c)
        {
            return c is >= '1' and <= '9';
        }

        private bool isEspace(char c)
        {
            return c is ' ' or '\n' or '\t';
        }

        private bool isEOF()
        {
            return pos >= content.Length;
        }

        private char nextChar()
        {
            if (isEOF())
            {
                return (char) 0;
            }

            return content[pos++];
        }

        private void back() => pos--;

        private bool isReservedKey(string term)
        {
            return (new[]
            {
                "program",
                "begin",
                "end",
                "real",
                "integer",
                "read",
                "write",
                "if",
                "then",
                "else",
            }).Contains(term);
        }

        private bool isAssign(char c)
        {
            return (new[]
            {
                ":",
                ":=",
            }).Contains(c.ToString());
        }

        private bool isArithmetic(char c)
        {
            return c is '+' or '-' or '*' or '/';
        }

        private bool isSymbol(char c)
        {
            return c is '(' or ')' or ';' or ',' or '.' or '$';
        }

        private bool isRelational(char c)
        {
            return (new[]
            {
                ">",
                "<",
                ">=",
                "=",
                "<=",
                "<>",
            }).Contains(c.ToString());
        }

        public Token nextToken()
        {
            if (isEOF())
            {
                return null;
            }
            char c;
            String term = "";
            state = 0;
            while (true)
            {
                if (isEOF())
                {
                    pos = content.Length + 1;
                }
                c = nextChar();
                switch (state)
                {
                    case 0:
                        if (isDigit(c))
                        {
                            state = 1;
                            term += c;
                        }
                        else if (isLetter(c))
                        {
                            state = 4;
                            term += c;

                        }
                        else if (isEspace(c))
                        {
                            state = 0;
                        }
                        else if (isArithmetic(c))
                        {
                            state = 5;
                            term += c;
                        }
                        else if (isRelational(c))
                        {
                            state = c switch
                            {
                                '<' => 6,
                                '>' => 8,
                                '=' => 10,
                                _ => state
                            };

                            term += c;
                        }
                        else if (isSymbol(c))
                        {
                            state = 11;
                            term += c;
                        }
                        else if (isAssign(c))
                        {
                            state = 12 ;
                            term += c;
                        }
                        break;
                    case 1:
                        if (isDigit(c))
                        {
                            state = 1;
                            term += c;
                        }
                        else if (c == '.')
                        {
                            state = 2;
                            term += c;
                        }
                        else if (!isLetter(c))
                        {
                            back();
                            return new Token(TokenEnum.INTEGER, term);
                        }
                        else
                        { 
                            throw new RuntimeBinderInternalCompilerException("Numero INTEGER invalido");
   
                        }

                        break;
                    case 2:
                        if (isDigit(c))
                        {
                            state = 3;
                            term += c;
                        }
                        else
                        {
                            throw new RuntimeBinderInternalCompilerException("Numero invalido");
                        }

                        break;
                    case 3:
                        if (isDigit(c))
                        {
                            state = 3;
                            term += c;
                        }
                        else if (!isLetter(c))
                        {
                            back();
                            return new Token(TokenEnum.REAL, term);
                        }
                        else
                        {
                            throw new RuntimeBinderInternalCompilerException("Numero REAL invalido");

                        }

                        break;
                    case 4:
                        
                        if (isDigit(c) || isLetter(c))
                        {
                            state = 4;
                            term += c;
                            Console.WriteLine(term);
                        }
                        else if (isReservedKey(term))
                        {
                            back();
                            return new Token(TokenEnum.RESERVED_KEY, term);
                        }
                        else
                        {
                            back();
                            return new Token(TokenEnum.IDENT, term);
                        }
                        break;
                    case 5:
                        back();
                        return new Token(TokenEnum.ARITHMETIC, term);
                    case 6:
                        if (c is '=' or '>')
                        {
                            term += c; 
                            state = 7;
                           
                        }
                        else
                        {
                            back();
                            return new Token(TokenEnum.RELATIONAL, term);
                        }
                        break;
                    case 7:
                        back();
                        return new Token(TokenEnum.RELATIONAL, term);
                    case 8:
                        if (c is '=')
                        {
                            term += c;
                            state = 9;
                        }
                        else
                        {
                            back();
                            return new Token(TokenEnum.RELATIONAL, term);
                        }
                        break;
                    case 9:
                        back();
                        return new Token(TokenEnum.RELATIONAL, term);
                    case 10:
                        back();
                        return new Token(TokenEnum.RELATIONAL, term);
                    case 11:
                        back();
                        return new Token(TokenEnum.SYMBOL, term);
                    case 12:
                        if (c == '=')
                        {
                            term += c;
                            state = 13;
                        }
                        else
                        {
                            back();
                            return new Token(TokenEnum.ASSIGN, term);
                        }
                        break;
                    case 13:
                        back();
                        return new Token(TokenEnum.ASSIGN, term);
                }
            }
        }
    }
}