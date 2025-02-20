using FileMutator.infrastructure.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileMutator.infrastructure.EF
{
    public class FileMutatorDbContext : DbContext
    {
        public DbSet<FileEntity> Files { get; set; } = null!;

        public FileMutatorDbContext(DbContextOptions<FileMutatorDbContext> options)
            : base(options)
        {
        }

        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileEntity>(typeBuilder =>
            {

                typeBuilder.HasKey(c => c.Id);
                typeBuilder.Property(i => i.Id).ValueGeneratedOnAdd();

            });
        }

    }
}
