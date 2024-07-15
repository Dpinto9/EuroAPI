namespace MinhaApi
{
    public class Conta
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Role { get; set; }

    }

    public class Estadio
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Capacidade { get; set; }
        public string Morada { get; set; }
        public string Cidade { get; set; }
    }

}
