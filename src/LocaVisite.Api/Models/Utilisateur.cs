using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Préposé, agent ou gestionnaire du système.</summary>
[Table("UTILISATEUR")]
public class Utilisateur
{
    [Key]
    [Column("id_utilisateur")]
    public int IdUtilisateur { get; set; }

    [Column("nom")]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Column("prenom")]
    [MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Column("courriel")]
    [MaxLength(200)]
    public string Courriel { get; set; } = string.Empty;

    /// <summary>Empreinte BCrypt. Nulle pour un compte Google ou Facebook.</summary>
    [Column("mot_de_passe")]
    [MaxLength(200)]
    public string? MotDePasse { get; set; }

    [Column("telephone")]
    [MaxLength(20)]
    public string? Telephone { get; set; }

    [Column("role")]
    public Role Role { get; set; }

    [Column("fournisseur_auth")]
    public FournisseurAuth FournisseurAuth { get; set; } = FournisseurAuth.LOCAL;

    /// <summary>Identifiant du compte chez le fournisseur externe.</summary>
    [Column("id_externe")]
    [MaxLength(200)]
    public string? IdExterne { get; set; }

    public ICollection<Disponibilite> Disponibilites { get; set; } = new List<Disponibilite>();

    /// <summary>Visites dont cet utilisateur est l'agent assigné.</summary>
    public ICollection<Visite> Visites { get; set; } = new List<Visite>();
}
