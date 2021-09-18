using System;

namespace compilador
{
     class Program
    {
        static void Main(string[] args)
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