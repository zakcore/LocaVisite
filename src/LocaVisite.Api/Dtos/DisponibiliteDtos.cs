using System.ComponentModel.DataAnnotations;
using LocaVisite.Api.Models;

namespace LocaVisite.Api.Dtos;

/// <summary>Agent tel que retourné au préposé.</summary>
public class AgentDto
{
    public int IdUtilisateur { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Courriel { get; set; } = string.Empty;
    public string? Telephone { get; set; }

    public static AgentDto Depuis(Utilisateur utilisateur) => new()
    {
        IdUtilisateur = utilisateur.IdUtilisateur,
        Nom = utilisateur.Nom,
        Prenom = utilisateur.Prenom,
        Courriel = utilisateur.Courriel,
        Telephone = utilisateur.Telephone
    };
}

/// <summary>Plage de disponibilité hebdomadaire telle que retournée par l'API.</summary>
public class DisponibiliteDto
{
    public int IdDisponibilite { get; set; }
    public int IdUtilisateur { get; set; }
    public int JourSemaine { get; set; }
    public string NomJour { get; set; } = string.Empty;
    public string HeureDebut { get; set; } = string.Empty;
    public string HeureFin { get; set; } = string.Empty;

    /// <summary>Noms des jours indexés comme <see cref="DayOfWeek"/> : 0 = dimanche.</summary>
    private static readonly string[] NomsJours =
        ["dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi"];

    public static DisponibiliteDto Depuis(Disponibilite disponibilite) => new()
    {
        IdDisponibilite = disponibilite.IdDisponibilite,
        IdUtilisateur = disponibilite.IdUtilisateur,
        JourSemaine = disponibilite.JourSemaine,
        NomJour = NomsJours[disponibilite.JourSemaine],
        HeureDebut = disponibilite.HeureDebut.ToString("HH\\:mm"),
        HeureFin = disponibilite.HeureFin.ToString("HH\\:mm")
    };
}

/// <summary>Plage de disponibilité saisie par le préposé.</summary>
public class DisponibiliteSaisieDto
{
    /// <summary>0 = dimanche, 6 = samedi.</summary>
    [Range(0, 6, ErrorMessage = "Le jour de la semaine doit être entre 0 (dimanche) et 6 (samedi).")]
    public int JourSemaine { get; set; }

    [Required(ErrorMessage = "L'heure de début est obligatoire.")]
    public TimeOnly HeureDebut { get; set; }

    [Required(ErrorMessage = "L'heure de fin est obligatoire.")]
    public TimeOnly HeureFin { get; set; }
}
