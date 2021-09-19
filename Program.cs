using System;

namespace compilador
{
     class Program
    {
        static void Main(string[] args)
        {
            int a = 0;
            if (a == 1)
            {
                Sintatico sintatico = new Sintatico("C:/Users/wilso/RiderProjects/compilador/input.txt");
                sintatico.analysis();
            }
            else
            {
                LexScanner scan = new LexScanner("C:/Users/wilso/RiderProjects/compilador/input.txt");
                Token token = null;
                while (true)
                {
                    token = scan.nextToken();
                    if (token == null)
                    {
                        break;
                    }

                    Console.WriteLine(token);
                }
            }
        }
    }
}