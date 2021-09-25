namespace compilador
{
    class Symbol
    {
        public string name { get; set; }
        public TokenEnum type { get; set; }

        public Symbol(TokenEnum type, string name)
        {
            this.name = name;
            this.type = type;
        }
    }
}