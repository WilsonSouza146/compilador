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
                using (var sr = new StreamReader(path))
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
            return pos == content.Length;
        }

        private char nextChar()
        {
            if (isEOF())
            {
                return (char) 0;
            }

            return content[pos++];
        }

        private void back()
        {
            if (!isEOF())
            {
                pos--;
            } 
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
                            throw new RuntimeBinderException("Numero INTEGER invalido");
   
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
                            throw new RuntimeBinderException("Numero invalido");
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
                            throw new RuntimeBinderException("Numero REAL invalido");

                        }

                        break;
                    case 4:
                        if (isDigit(c) || isLetter(c))
                        {
                            state = 4;
                            term += c;
                        }
                        else
                        {
                            back();
                            return new Token(TokenEnum.IDENT, term);
                        }
                        break;
                }
            }
        }
    }
}