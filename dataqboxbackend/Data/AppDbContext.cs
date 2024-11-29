using Microsoft.EntityFrameworkCore;
using dataqboxbackend.Models;

namespace dataqboxbackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
      
    }
} 