using System.ComponentModel.DataAnnotations;
using LocaVisite.Api.Models;

namespace LocaVisite.Api.Dtos;

/// <summary>Demande de visite soumise par un prospect, sans authentification.</summary>
public class DemandeVisiteDto
{
    [Required(ErrorMessage = "Le logement est obligatoire.")]
    public int IdLogement { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères.")]
    public string? Prenom { get; set; }

    [Required(ErrorMessage = "Le courriel est obligatoire.")]
    [EmailAddress(ErrorMessage = "Le format du courriel n'est pas valide.")]
    [MaxLength(200, ErrorMessage = "Le courriel ne peut pas dépasser 200 caractères.")]
    public string Courriel { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères.")]
    public string? Telephone { get; set; }

    public bool PossedeMobile { get; set; }

    [Required(ErrorMessage = "La date souhaitée est obligatoire.")]
    public DateOnly DateSouhaitee { get; set; }

    [Required(ErrorMessage = "L'heure de début est obligatoire.")]
    public TimeOnly HeureSouhaiteeDebut { get; set; }

    [Required(ErrorMessage = "L'heure de fin est obligatoire.")]
    public TimeOnly HeureSouhaiteeFin { get; set; }
}

/// <summary>Accusé de réception retourné au prospect après sa demande.</summary>
public class ReponseDemandeVisiteDto
{
    public int IdVisite { get; set; }
    public int IdProspect { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>Prospect tel que retourné par l'API.</summary>
public class ProspectDto
{
    public int IdProspect { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Courriel { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public bool PossedeMobile { get; set; }

    public static ProspectDto Depuis(Prospect prospect) => new()
    {
        IdProspect = prospect.IdProspect,
        Nom = prospect.Nom,
        Prenom = prospect.Prenom,
        Courriel = prospect.Courriel,
        Telephone = prospect.Telephone,
        PossedeMobile = prospect.PossedeMobile
    };
}

/// <summary>Visite telle que retournée au préposé, avec son logement et son prospect.</summary>
public class VisiteDto
{
    public int IdVisite { get; set; }
    public string Statut { get; set; } = string.Empty;

    public DateOnly DateSouhaitee { get; set; }
    public string HeureSouhaiteeDebut { get; set; } = string.Empty;
    public string HeureSouhaiteeFin { get; set; } = string.Empty;

    public DateOnly? DatePrevue { get; set; }
    public string? HeurePrevue { get; set; }
    public int DureePrevue { get; set; }

    /// <summary>Nul tant que la visite n'est pas assignée.</summary>
    public int? IdAgent { get; set; }

    public LogementDto? Logement { get; set; }
    public ProspectDto? Prospect { get; set; }

    public static VisiteDto Depuis(Visite visite) => new()
    {
        IdVisite = visite.IdVisite,
        Statut = visite.Statut.ToString(),
        DateSouhaitee = visite.DateSouhaitee,
        HeureSouhaiteeDebut = visite.HeureSouhaiteeDebut.ToString("HH\\:mm"),
        HeureSouhaiteeFin = visite.HeureSouhaiteeFin.ToString("HH\\:mm"),
        DatePrevue = visite.DatePrevue,
        HeurePrevue = visite.HeurePrevue?.ToString("HH\\:mm"),
        DureePrevue = visite.DureePrevue,
        IdAgent = visite.IdAgent,
        Logement = visite.Logement is null ? null : LogementDto.Depuis(visite.Logement),
        Prospect = visite.Prospect is null ? null : ProspectDto.Depuis(visite.Prospect)
    };
}

/// <summary>Durée prévue fixée par le préposé, en minutes.</summary>
public class DureeVisiteDto
{
    [Range(15, 240, ErrorMessage = "La durée doit être entre 15 et 240 minutes.")]
    public int DureePrevue { get; set; }
}
