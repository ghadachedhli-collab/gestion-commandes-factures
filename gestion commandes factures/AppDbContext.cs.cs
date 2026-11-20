using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Produit> Produits => Set<Produit>();

    // gestion.db sera placé à la racine du projet (à côté du .csproj)
    private static string CheminBase()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && dir.GetFiles("*.csproj").Length == 0)
            dir = dir.Parent;
        var racine = dir?.FullName ?? AppContext.BaseDirectory;
        return Path.Combine(racine, "gestion.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={CheminBase()}");

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.IdClient);
            entity.Property(c => c.Nom).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(160);
            entity.Property(c => c.Telephone).IsRequired().HasMaxLength(30);
            entity.Property(c => c.Adresse).IsRequired().HasMaxLength(200);
        });

        mb.Entity<Produit>(entity =>
        {
            entity.HasKey(p => p.IdProduit);
            entity.Property(p => p.Designation).IsRequired().HasMaxLength(120);
            entity.Property(p => p.PrixUnitaire).HasPrecision(18, 2);
        });
    }
}