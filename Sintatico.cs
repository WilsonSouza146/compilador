using System;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace compilador
{
    class Sintatico
    {
        private LexScanner lexico;
        private Token token;

        public Sintatico(string path)
        {
            lexico = new LexScanner(path);
        }

        private bool verifyToken(string term)
        {
            return (token != null && token.term.Equals(term));
        }

        public void analysis()
        {
            programa();
            if (token == null)
            {
                Console.WriteLine("Deu bom");
            }
            else
            {
                throw new RuntimeBinderInternalCompilerException($"Erro sintatico esperado fim de cadeia e foi encontrado: {token.term}");
            }
        }

        private void getToken()
        {
            token = lexico.nextToken();
            Console.WriteLine($"Token = {token}");
        }


        private void programa()
        {
            Console.WriteLine("programa");
            getToken();
            if (verifyToken("program"))
            {
                getToken();
                if (token.type == TokenEnum.IDENT)
                {
                    getToken();
                    corpo();
                    getToken();
                    if (!token.term.Equals("."))
                    {
                        throw new Exception($"Erro sintatico, esperado '.' e recebido {token}");
                    }
                    token = null;
                }
            }
        }

        private void corpo()
        {
            Console.WriteLine("corpo");
            dc();
            if (verifyToken("begin"))
            {
                getToken();
                comandos(); 
                if (!verifyToken("end"))
                {
                    throw new Exception($"Erro sintatico esperado 'end' e foi encontrado: {token.term}");
                } 
            }
            else
            {
                throw new Exception($"Erro sintatico esperado 'begin' e foi encontrado: {token.term}");

            }
        }

        private void dc()
        {
            Console.WriteLine("dc");
            if (!verifyToken("begin"))
            {
                dc_v();
                mais_dc();
            }
        }
        
        private void mais_dc()
        {
            Console.WriteLine("mais_dc");
            if (token.term.Equals(";"))
            {
                getToken();
                dc();
            }
            else
            {
                throw new Exception($"Erro sintatico esperado ';' recebido {token}");
            }
        }
        private void dc_v()
        {
            Console.WriteLine("dc_v");
            tipo_var();
            getToken();
            if (token.type == TokenEnum.ASSIGN)
            { 
                getToken();
                variaveis();
            }
            else
            { 
                throw new Exception($"Erro sintatico, esperado ':' e foi encontrado: {token}");

            }
        }

        private void tipo_var()
        {
            Console.WriteLine("tipo_var");
            if (!(verifyToken("integer") || verifyToken("real")))
            {
                throw new Exception($"Erro sintatico, esperado 'INTEGER' ou 'REAL' e foi encontrado: {token}");

            }
        }

        private void variaveis()
        {
            Console.WriteLine("variaveis");
            if (token.type == TokenEnum.IDENT)
            {
                getToken();
                mais_var(); 
            }
            else
            {
                throw new Exception($"Erro sintatico esperado 'IDENT' e foi encontrado: {token}");

            }
        }

        private void mais_var()
        {
            Console.WriteLine("mais_var");
            if (token.term.Equals(","))
            {
                getToken();
                variaveis();
            }
        }
        
        private void comandos()
        {
            Console.WriteLine("comandos");
            comando();
            mais_comandos();
        }
        
        private void comando()
        {
            Console.WriteLine("comando");
            if (verifyToken("read") || verifyToken("write"))
            {
                verif_parenteses();
            }
            else if (token.type == TokenEnum.IDENT)
            {
                
            }
            else
            {
                throw new Exception($"Erro sintatico esperado um comando e foi encontrado: {token}");

            }
        }

        private void mais_comandos()
        {
            Console.WriteLine("mais_comandos");
            getToken();
            if (token.term == ";")
            {
                getToken();
                if (!verifyToken("end"))
                {
                    comandos();
                }
            }
            else
            {
                throw new Exception($"Erro sintatico esperado ';' recebido {token}");
            }
        }

        private void verif_parenteses()
        {
            getToken();
            if (token.term.Equals("("))
            {
                getToken();
                if (token.type == TokenEnum.IDENT)
                {
                    getToken();
                    if (!(token.term.Equals(")")))
                    {
                        throw new Exception($"Erro sintatico, esperado ')'  e foi encontrado {token}");
                    }
                }
                else
                {
                    throw new Exception($"Erro sintatico, esperado 'IDENT'  e foi encontrado {token}");
                }
            }
            else
            {
                throw new Exception($"Erro sintatico, esperado '('  e foi encontrado {token}");
            }
        }
    }
}