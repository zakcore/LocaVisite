using LocaVisite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Data;

/// <summary>Contexte Entity Framework de la base LocaVisite.</summary>
public class LocaVisiteContext : DbContext
{
    public LocaVisiteContext(DbContextOptions<LocaVisiteContext> options) : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Disponibilite> Disponibilites => Set<Disponibilite>();
    public DbSet<Logement> Logements => Set<Logement>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Prospect> Prospects => Set<Prospect>();
    public DbSet<Visite> Visites => Set<Visite>();
    public DbSet<RapportVisite> RapportsVisite => Set<RapportVisite>();
    public DbSet<Communication> Communications => Set<Communication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Énumérations stockées en texte plutôt qu'en entier,
        //     pour que la base reste lisible directement en SQL. ---
        modelBuilder.Entity<Utilisateur>().Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Utilisateur>().Property(u => u.FournisseurAuth).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Logement>().Property(l => l.Statut).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Visite>().Property(v => v.Statut).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<RapportVisite>().Property(r => r.NiveauInteret).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Communication>().Property(c => c.Type).HasConversion<string>().HasMaxLength(20);

        // --- Précision des montants ---
        modelBuilder.Entity<Logement>().Property(l => l.LoyerMensuel).HasPrecision(10, 2);
        modelBuilder.Entity<Visite>().Property(v => v.DistanceKm).HasPrecision(6, 2);

        // --- Deux comptes ne peuvent pas partager la même adresse courriel. ---
        modelBuilder.Entity<Utilisateur>()
            .HasIndex(u => u.Courriel)
            .IsUnique();

        // --- Un agent a plusieurs disponibilités. ---
        modelBuilder.Entity<Disponibilite>()
            .HasOne(d => d.Utilisateur)
            .WithMany(u => u.Disponibilites)
            .HasForeignKey(d => d.IdUtilisateur)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Logement)
            .WithMany(l => l.Photos)
            .HasForeignKey(p => p.IdLogement)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Visite>()
            .HasOne(v => v.Logement)
            .WithMany(l => l.Visites)
            .HasForeignKey(v => v.IdLogement)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visite>()
            .HasOne(v => v.Prospect)
            .WithMany(p => p.Visites)
            .HasForeignKey(v => v.IdProspect)
            .OnDelete(DeleteBehavior.Restrict);

        // --- L'agent est facultatif et ne doit jamais être supprimé en cascade :
        //     l'historique des visites doit survivre au départ d'un agent. ---
        modelBuilder.Entity<Visite>()
            .HasOne(v => v.Agent)
            .WithMany(u => u.Visites)
            .HasForeignKey(v => v.IdAgent)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Relation 1—0..1 : au plus un rapport par visite. ---
        modelBuilder.Entity<RapportVisite>()
            .HasOne(r => r.Visite)
            .WithOne(v => v.RapportVisite)
            .HasForeignKey<RapportVisite>(r => r.IdVisite)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Communication>()
            .HasOne(c => c.Visite)
            .WithMany(v => v.Communications)
            .HasForeignKey(c => c.IdVisite)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
