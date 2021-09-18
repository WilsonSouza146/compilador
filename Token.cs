using System; 

namespace compilador
{
    class Token
    {

        public Token(TokenEnum type, string term)
            {
                this.type = type;
                this.term = term;
            }
        
        public TokenEnum type { get; set; }
        public string term{get; set; }

        public override string ToString()
        {
            return $"Token[{type},{term}]";
        }

    }
}