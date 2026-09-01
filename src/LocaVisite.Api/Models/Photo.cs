using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Photo associée à un logement.</summary>
[Table("PHOTO")]
public class Photo
{
    [Key]
    [Column("id_photo")]
    public int IdPhoto { get; set; }

    [Column("id_logement")]
    public int IdLogement { get; set; }

    [Column("chemin_fichier")]
    [MaxLength(300)]
    public string CheminFichier { get; set; } = string.Empty;

    [Column("ordre_affichage")]
    public int OrdreAffichage { get; set; }

    public Logement? Logement { get; set; }
}
