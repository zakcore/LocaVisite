using System.ComponentModel.DataAnnotations;
using LocaVisite.Api.Models;

namespace LocaVisite.Api.Dtos;

/// <summary>Logement tel que retourné par l'API.</summary>
public class LogementDto
{
    public int IdLogement { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int NbPieces { get; set; }
    public decimal LoyerMensuel { get; set; }
    public string? Description { get; set; }
    public string Statut { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public static LogementDto Depuis(Logement logement) => new()
    {
        IdLogement = logement.IdLogement,
        Adresse = logement.Adresse,
        Ville = logement.Ville,
        CodePostal = logement.CodePostal,
        Type = logement.Type,
        NbPieces = logement.NbPieces,
        LoyerMensuel = logement.LoyerMensuel,
        Description = logement.Description,
        Statut = logement.Statut.ToString(),
        Latitude = logement.Latitude,
        Longitude = logement.Longitude
    };
}

/// <summary>Données acceptées à la création et à la modification d'un logement.</summary>
public class LogementSaisieDto
{
    [Required(ErrorMessage = "L'adresse est obligatoire.")]
    [MaxLength(200, ErrorMessage = "L'adresse ne peut pas dépasser 200 caractères.")]
    public string Adresse { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ville est obligatoire.")]
    [MaxLength(100, ErrorMessage = "La ville ne peut pas dépasser 100 caractères.")]
    public string Ville { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code postal est obligatoire.")]
    [MaxLength(10, ErrorMessage = "Le code postal ne peut pas dépasser 10 caractères.")]
    public string CodePostal { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le type de logement est obligatoire.")]
    [MaxLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères.")]
    public string Type { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Le nombre de pièces doit être entre 1 et 20.")]
    public int NbPieces { get; set; }

    [Range(0, 100000, ErrorMessage = "Le loyer mensuel doit être entre 0 et 100 000 $.")]
    public decimal LoyerMensuel { get; set; }

    public string? Description { get; set; }

    /// <summary>Facultatif à la création : le logement est DISPONIBLE par défaut.</summary>
    public StatutLogement? Statut { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
