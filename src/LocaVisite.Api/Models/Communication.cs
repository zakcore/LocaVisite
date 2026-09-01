using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Message envoyé au prospect au sujet d'une visite.</summary>
[Table("COMMUNICATION")]
public class Communication
{
    [Key]
    [Column("id_communication")]
    public int IdCommunication { get; set; }

    [Column("id_visite")]
    public int IdVisite { get; set; }

    [Column("type")]
    public TypeCommunication Type { get; set; }

    [Column("contenu")]
    public string? Contenu { get; set; }

    [Column("date_envoi")]
    public DateTime DateEnvoi { get; set; } = DateTime.Now;

    /// <summary>Résultat rapporté par la passerelle, par exemple « ENVOYE » ou « ECHEC ».</summary>
    [Column("statut_envoi")]
    [MaxLength(50)]
    public string? StatutEnvoi { get; set; }

    public Visite? Visite { get; set; }
}
