using System.ComponentModel.DataAnnotations;

namespace LocaVisite.Api.Dtos;

/// <summary>
/// Réponse de la recherche d'agents disponibles.
/// Le format suit le contrat d'endpoint décrit dans SPECIFICATIONS.md.
/// </summary>
public class AgentsDisponiblesDto
{
    /// <summary>Durée de la visite, en minutes.</summary>
    public int DureeRequise { get; set; }

    public PlageDemandeeDto PlageDemandee { get; set; } = new();

    public List<CreneauDto> Creneaux { get; set; } = [];

    /// <summary>
    /// Règle 5 : quand aucun créneau n'est retenu, la raison explique pourquoi.
    /// Nulle dès qu'il y a au moins un créneau.
    /// </summary>
    public string? Raison { get; set; }
}

/// <summary>Plage horaire proposée par le prospect, rappelée dans la réponse.</summary>
public class PlageDemandeeDto
{
    public DateOnly Date { get; set; }
    public string Debut { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
}

/// <summary>
/// Règle 1 : le résultat est un couple (agent, heure de début), pas un simple
/// nom d'agent. Le préposé a besoin de l'heure à retenir.
/// </summary>
public class CreneauDto
{
    public int IdAgent { get; set; }
    public string NomAgent { get; set; } = string.Empty;
    public string HeureDebut { get; set; } = string.Empty;
    public string HeureFin { get; set; } = string.Empty;
}

/// <summary>Créneau choisi par le préposé au moment d'assigner la visite.</summary>
public class AssignationDto
{
    [Required(ErrorMessage = "L'agent est obligatoire.")]
    public int IdAgent { get; set; }

    [Required(ErrorMessage = "L'heure de début du créneau est obligatoire.")]
    public TimeOnly HeureDebut { get; set; }
}
