using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace compilador
{
    class Sintatico
    {
        private LexScanner lexico;
        private Token token;
        private Dictionary<string, Symbol> tableSymbol = new Dictionary<string, Symbol>();
        private TokenEnum type, typeExp;
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
            if(token.term.Equals("integer"))
            {
                type = TokenEnum.INTEGER;
            }
            else
            {
                type = TokenEnum.REAL;
            }
        }

        private void variaveis()
        {
            Console.WriteLine("variaveis");
            if (token.type == TokenEnum.IDENT)
            {
                if (tableSymbol.ContainsKey(token.term))
                {
                    throw new Exception($"Erro semantico, identificador ja encontrado: {token.term}");
                }
                else
                {
                    tableSymbol.Add(token.term, new Symbol(type, token.term));
                }
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
                verif_table_symbol();
                getTypeExp();
                getToken();
                if (token.type == TokenEnum.ASSIGN)
                {
                    getToken();
                    expressao();
                }
            }
            else if (verifyToken("if"))
            {
                getToken();
                getTypeExp();
                condicao();
                if (verifyToken("then"))
                {
                    getToken();
                    comandos();
                    pfalsa();
                   if (token.term.Equals("$"))
                   {
                       getToken();
                   }
                }
            }
            else
            {
                throw new Exception($"Erro sintatico esperado um comando e foi encontrado: {token}");

            }
        }

        private void mais_comandos()
        {
            Console.WriteLine("mais_comandos");
            if (token.term == ";")
            {
                getToken();
                if (!verifyToken("end"))
                {
                    comandos();
                }
            }
        }

        private void expressao()
        {
            
            Console.WriteLine("expressao");
            verif_table_symbol();
            if (typeExp != tableSymbol[token.term].type)
            {
                throw new Exception(
                    $"Erro semantico, variavel '{token.term}'do tipo {tableSymbol[token.term].type} sendo usada em expressao do tipo {typeExp}");
            }
            termo();
            outros_termos();

        }

        private void termo()
        {
            Console.WriteLine("termo");
            op_un();
            fator();
            mais_fatores();
        }

        private void op_un()
        {
            
            Console.WriteLine("op_un");

            if (token.term.Equals("-"))
            {
                getToken();
            }
        }

        private void fator()
        {
            Console.WriteLine("fator");
            if (token.type is TokenEnum.IDENT or TokenEnum.REAL or TokenEnum.INTEGER)
            {
               
                if (token.type == TokenEnum.IDENT)
                {
                    verif_table_symbol();
                }
                getTypeExp();
                getToken();
            }

            else if (!token.term.Equals(";"))
            {
                getToken();
                if (token.term.Equals("("))
                {
                    expressao();
                    getToken();
                    if (token.term.Equals(")"))
                    {
                        getToken();
                    }
                }
            }
        }

        private void mais_fatores()
        {
            Console.WriteLine("mais_fatores");
            
            if (token.term is "*" or "/")
            {
                op_mul();
                fator();
                mais_fatores();
            }
        }

        private void outros_termos()
        {
            Console.WriteLine("outros_termos");

            if (token.term is "+" or "-")
            {
                op_ad();
                termo();
                outros_termos();
            }
        }

        private void op_mul()
        {
            Console.WriteLine("op_mul");

            if (token.term is "*" or "/")
            {
                getToken();
            }
            else
            {
                throw new Exception($"Erro sintatico, esperado '*' ou '/' recebido {token}");

            }
        }

        private void op_ad()
        {
            Console.WriteLine("op_ad");

            if (token.term is "+" or "-")
            {
                getToken();
            }
            else
            {
                throw new Exception($"Erro sintatico, esperado '+' ou '-' recebido {token}");

            }
        }

        private void condicao()
        {
            expressao();
            relacao();
            expressao();
        }

        private void relacao()
        {
            Console.WriteLine("relacao");
            if (token.type == TokenEnum.RELATIONAL)
            {
                getToken();
            }
        }

        private void pfalsa()
        {
            Console.WriteLine("pfalsa");
            if (!token.term.Equals("$"))
            {
                if (verifyToken("else"))
                {
                    getToken();
                    comandos();
                }
            }
        }

        private void verif_parenteses()
        {
            Console.WriteLine("verif_parenteses");
            getToken();
            if (token.term.Equals("("))
            {
                getToken();
                if (token.type == TokenEnum.IDENT)
                {
                    verif_table_symbol();
                    getToken();
                    if (!(token.term.Equals(")")))
                    {
                        throw new Exception($"Erro sintatico, esperado ')'  e foi encontrado {token}");
                    }
                    getToken();
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
        
        private void verif_table_symbol()
        {
            if (!tableSymbol.ContainsKey(token.term))
            {
                throw new Exception($"Erro semantico, variavel '{token}' sendo usada e nao foi declarada"); 
            }
        }

        private void getTypeExp()
        {
            Console.WriteLine("getTypeExp");
            typeExp = tableSymbol[token.term].type;
            Console.WriteLine($"TypeEXP = {typeExp}");
        }
    }
}