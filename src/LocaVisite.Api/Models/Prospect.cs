using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Personne qui demande à visiter un logement. Ne possède pas de compte.</summary>
[Table("PROSPECT")]
public class Prospect
{
    [Key]
    [Column("id_prospect")]
    public int IdProspect { get; set; }

    [Column("nom")]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Column("prenom")]
    [MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Column("courriel")]
    [MaxLength(200)]
    public string Courriel { get; set; } = string.Empty;

    [Column("telephone")]
    [MaxLength(20)]
    public string? Telephone { get; set; }

    /// <summary>Indique si le prospect peut recevoir un rappel par SMS.</summary>
    [Column("possede_mobile")]
    public bool PossedeMobile { get; set; }

    [Column("date_creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    public ICollection<Visite> Visites { get; set; } = new List<Visite>();
}
