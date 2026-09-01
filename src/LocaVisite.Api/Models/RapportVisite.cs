using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Compte rendu rédigé par l'agent après une visite. Au plus un par visite.</summary>
[Table("RAPPORT_VISITE")]
public class RapportVisite
{
    [Key]
    [Column("id_rapport")]
    public int IdRapport { get; set; }

    [Column("id_visite")]
    public int IdVisite { get; set; }

    [Column("commentaire")]
    public string? Commentaire { get; set; }

    [Column("niveau_interet")]
    public NiveauInteret NiveauInteret { get; set; }

    /// <summary>Faux si le prospect ne s'est pas présenté.</summary>
    [Column("prospect_present")]
    public bool ProspectPresent { get; set; }

    [Column("date_creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    public Visite? Visite { get; set; }
}
