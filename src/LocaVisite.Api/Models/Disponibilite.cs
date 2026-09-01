using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>Plage horaire hebdomadaire pendant laquelle un agent peut faire des visites.</summary>
[Table("DISPONIBILITE")]
public class Disponibilite
{
    [Key]
    [Column("id_disponibilite")]
    public int IdDisponibilite { get; set; }

    [Column("id_utilisateur")]
    public int IdUtilisateur { get; set; }

    /// <summary>0 = dimanche, 6 = samedi, comme <see cref="DayOfWeek"/>.</summary>
    [Column("jour_semaine")]
    public int JourSemaine { get; set; }

    [Column("heure_debut")]
    public TimeOnly HeureDebut { get; set; }

    [Column("heure_fin")]
    public TimeOnly HeureFin { get; set; }

    public Utilisateur? Utilisateur { get; set; }
}
