using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace compilador
{
    class Sintatico
    {
        private LexScanner lexico;
        private Token token;
        private Dictionary<string, Symbol> tableSymbol = new Dictionary<string, Symbol>();
        private TokenEnum type, typeExp;
        private int temp = 1;
        private StringBuilder codeLine = new StringBuilder("cont - op ; arg1 ; arg2 ; result\n");
        private int cont, i, linha;
        public Sintatico(string path)
        {
            lexico = new LexScanner(path);
        }

        private string geraTemp()
        {
            return "t" + temp++;
        }

        private string getLine()
        {
            i++;
            linha = i;
            return "&" + i ;
        }
        
        private void replace_getLine(int cont)
        {
            codeLine.Replace("&" + linha, cont.ToString());
            linha--;
        }

        private void code(string op, string arg1, string arg2, string result)
        {
            codeLine.Append(cont +" - "+ op + " ; " + arg1 + " ; " + arg2 + " ; " + result + "\n");
            cont++; 
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
                    code("PARA","","","");
                    Console.WriteLine(codeLine);
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
        
        private void dc_v()
        {
            Console.WriteLine("dc_v");
            string tipo_dir = tipo_var();
            getToken();
            if (token.type == TokenEnum.ASSIGN)
            { 
                getToken();
                variaveis(tipo_dir);
            }
            else
            { 
                throw new Exception($"Erro sintatico, esperado ':' e foi encontrado: {token}");

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

        private string tipo_var()
        {
            Console.WriteLine("tipo_var");
            if (!(verifyToken("integer") || verifyToken("real")))
            {
                throw new Exception($"Erro sintatico, esperado 'INTEGER' ou 'REAL' e foi encontrado: {token}");

            }
            if(token.term.Equals("integer"))
            {
                type = TokenEnum.INTEGER;
                return "0";
            }
            else
            {
                type = TokenEnum.REAL;
                return "0.0";
            }
        }

        private void variaveis(string var_esq)
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
                code("ALME", var_esq, "", token.term);
                getToken();
                mais_var(var_esq); 
            }

            else
            {
                throw new Exception($"Erro sintatico esperado 'IDENT' e foi encontrado: {token}");

            }
        }

        private void mais_var(string mais_var_esq)
        {
            Console.WriteLine("mais_var");
            if (token.term.Equals(","))
            {
                getToken();
                variaveis(mais_var_esq);
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
                string op = token.term;
                verif_parenteses(op);
            }
            else if (token.type == TokenEnum.IDENT)
            {
                verif_table_symbol();
                getTypeExp();
                string ident = token.term;
                getToken();
                if (token.type == TokenEnum.ASSIGN)
                {
                    getToken();
                    string dir = expressao();
                    code(":=",dir, "", ident);
                }
            }
            else if (verifyToken("if"))
            {
                getToken();
                getTypeExp();
                string condicao_dir = condicao();
                if (verifyToken("then"))
                {
                    getToken();
                    code("JF", condicao_dir, getLine(), "");
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

        private string expressao()
        {
            
            Console.WriteLine("expressao");
            if (token.type == TokenEnum.IDENT)
            {
                verif_table_symbol();
                if (typeExp != tableSymbol[token.term].type)
                {
                    throw new Exception(
                        $"Erro semantico, variavel '{token.term}'do tipo {tableSymbol[token.term].type} sendo usada em expressao do tipo {typeExp}");
                }
            }
            else
            {
                if (token.type != typeExp && token.term != "-")
                {
                    throw  new Exception($"Erro semantico, variavel '{token.term}'do tipo {token.type} sendo usada em expressao do tipo {typeExp}");
                }
            }
            string termo_dir = termo();
            string outros_termos_dir = outros_termos(termo_dir);
            return outros_termos_dir;
        }

        private string termo()
        {
            Console.WriteLine("termo");
            string op_minus = op_un();
            string fator_dir = fator();
            if (op_minus.Equals("-"))
            {
                string fator1_dir = geraTemp();
                code(op_minus, fator_dir, "", fator1_dir);
                string mais_fatores_dir = mais_fatores(fator1_dir);
                return mais_fatores_dir;
            }
            else
            {
                string mais_fatores_dir = mais_fatores(fator_dir);
                return mais_fatores_dir;
            }
        }

        private string op_un()
        {
            
            Console.WriteLine("op_un");

            if (token.term.Equals("-"))
            {
                string op_un_dir = token.term;
                getToken();
                return op_un_dir;
            }

            return "";
        }

        private string fator()
        {
            Console.WriteLine("fator");
            if (token.type is TokenEnum.IDENT)
            {
                verif_table_symbol();
                getTypeExp();
                Token id = token;
                getToken();
                return tableSymbol[id.term].name;
                
            }
            
            else if (token.type is TokenEnum.REAL or TokenEnum.INTEGER)
            {
                Token id = token;
                getToken();
                return id.term;
            }

            if (!token.term.Equals(";"))
            {
                getToken();
                if (token.term.Equals("("))
                {
                    getToken();
                    string expressao_dir = expressao();
                    if (token.term.Equals(")"))
                    {
                        getToken();
                        return expressao_dir;
                    }
                }
            }

            return "";
        }

        private string mais_fatores(string fator_esq)
        {
            Console.WriteLine("mais_fatores");
            
            if (token.term is "*" or "/")
            {
                op_mul();
                string fator_dir = fator();
                string mais_fatores1_dir = mais_fatores(fator_dir);
                string mais_fatores_dir = geraTemp();
                code("*", fator_esq, mais_fatores1_dir, mais_fatores_dir);
                return mais_fatores_dir;
            }
            else
            {
                return fator_esq;
            }
        }

        private string outros_termos(string outros_termos_esq)
        {
            Console.WriteLine("outros_termos");

            if (token.term is "+" or "-")
            {
                op_ad();
                string termo_dir =  termo();
                string outros_termos1_dir = outros_termos(termo_dir);
                string outros_termos_dir = geraTemp();
                code("+", outros_termos_esq, outros_termos1_dir, outros_termos_dir);
                return outros_termos_dir;
            }

            return outros_termos_esq;
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

        private string condicao()
        {
            string expressao_dir = expressao();
            string relacao_dir = relacao();
            string expressao1_dir =expressao();
            string condicao_dir = geraTemp();
            code(relacao_dir, expressao_dir, expressao1_dir, condicao_dir);
            return condicao_dir;
        }

        private string relacao()
        {
            Console.WriteLine("relacao");
            if (token.type == TokenEnum.RELATIONAL)
            {
                string op = token.term;
                getToken();
                return op;
            }

            return "";
        }

        private void pfalsa()
        {
            Console.WriteLine("pfalsa");
            if (!token.term.Equals("$"))
            {
                replace_getLine(cont+1);
                if (verifyToken("else"))
                {
                    replace_getLine(cont);
                    code("goto", getLine(), "", "");
                    getToken();
                    comandos();
                    replace_getLine(cont);
                }
            }
        }

        private void verif_parenteses(string op)
        {
            Console.WriteLine("verif_parenteses");
            getToken();
            if (token.term.Equals("("))
            {
                getToken();
                if (token.type == TokenEnum.IDENT)
                {
                    verif_table_symbol();
                    if (op == "read")
                    {
                        Console.WriteLine(token.term);
                        code(op, "", "", token.term);
                    }
                    else
                    {
                        code(op, token.term, "", "");
                    }
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