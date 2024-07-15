using Microsoft.EntityFrameworkCore;

namespace MinhaApi
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Conta> Contas { get; set; }
        public DbSet<Estadio> Estadios { get; set; }
    }
}
