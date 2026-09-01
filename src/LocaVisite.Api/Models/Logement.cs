using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Logement offert en location.</summary>
[Table("LOGEMENT")]
public class Logement
{
    [Key]
    [Column("id_logement")]
    public int IdLogement { get; set; }

    [Column("adresse")]
    [MaxLength(200)]
    public string Adresse { get; set; } = string.Empty;

    [Column("ville")]
    [MaxLength(100)]
    public string Ville { get; set; } = string.Empty;

    [Column("code_postal")]
    [MaxLength(10)]
    public string CodePostal { get; set; } = string.Empty;

    /// <summary>Type de logement, par exemple « Appartement » ou « Maison ».</summary>
    [Column("type")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Column("nb_pieces")]
    public int NbPieces { get; set; }

    [Column("loyer_mensuel")]
    public decimal LoyerMensuel { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("statut")]
    public StatutLogement Statut { get; set; } = StatutLogement.DISPONIBLE;

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    public ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public ICollection<Visite> Visites { get; set; } = new List<Visite>();
}
