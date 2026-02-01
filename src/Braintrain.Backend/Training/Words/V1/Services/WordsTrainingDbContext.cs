using Braintrain.Backend.Training.Words.V1.Entities;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Braintrain.Backend.Training.Words.V1.Services;

public class WordsTrainingDbContext : DbContext
{
    public WordsTrainingDbContext(DbContextOptions<WordsTrainingDbContext> options) : base(options)
    {

    }
    public DbSet<Word> Words => Set<Word>();
    public DbSet<Entities.Color> Colors => Set<Entities.Color>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Word>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Word>()
            .HasIndex(u => u.WordValue);

        modelBuilder.Entity<Entities.Color > ()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Entities.Color>()
            .HasIndex(u => u.ColorName);
    }
}
