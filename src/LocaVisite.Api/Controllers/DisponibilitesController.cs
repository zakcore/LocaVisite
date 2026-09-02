using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Controllers;

/// <summary>
/// Agents et plages de disponibilité hebdomadaires.
/// Les routes portent sur /api/agents et /api/disponibilites, d'où les
/// gabarits complets sur chaque action.
/// </summary>
[ApiController]
[Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
public class DisponibilitesController : ControllerBase
{
    private readonly LocaVisiteContext _contexte;

    public DisponibilitesController(LocaVisiteContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>Liste des utilisateurs de rôle AGENT.</summary>
    [HttpGet("api/agents")]
    public async Task<ActionResult<IEnumerable<AgentDto>>> ListerAgents()
    {
        var agents = await _contexte.Utilisateurs
            .Where(u => u.Role == Role.AGENT)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();

        return Ok(agents.Select(AgentDto.Depuis));
    }

    /// <summary>Plages hebdomadaires déclarées par un agent.</summary>
    [HttpGet("api/agents/{id:int}/disponibilites")]
    public async Task<ActionResult<IEnumerable<DisponibiliteDto>>> ListerDisponibilites(int id)
    {
        var erreur = await ValiderAgent(id);
        if (erreur is not null)
        {
            return erreur;
        }

        var disponibilites = await _contexte.Disponibilites
            .Where(d => d.IdUtilisateur == id)
            .OrderBy(d => d.JourSemaine)
            .ThenBy(d => d.HeureDebut)
            .ToListAsync();

        return Ok(disponibilites.Select(DisponibiliteDto.Depuis));
    }

    /// <summary>Ajoute une plage de disponibilité à un agent.</summary>
    [HttpPost("api/agents/{id:int}/disponibilites")]
    public async Task<ActionResult<DisponibiliteDto>> AjouterDisponibilite(
        int id,
        [FromBody] DisponibiliteSaisieDto saisie)
    {
        var erreur = await ValiderAgent(id);
        if (erreur is not null)
        {
            return erreur;
        }

        if (saisie.HeureFin <= saisie.HeureDebut)
        {
            return BadRequest(new { message = "L'heure de fin doit être postérieure à l'heure de début." });
        }

        // Deux plages se chevauchent dès que l'une commence avant que l'autre
        // ne finisse, dans les deux sens.
        var plagesDuJour = await _contexte.Disponibilites
            .Where(d => d.IdUtilisateur == id && d.JourSemaine == saisie.JourSemaine)
            .ToListAsync();

        var chevauchement = plagesDuJour
            .FirstOrDefault(d => saisie.HeureDebut < d.HeureFin && d.HeureDebut < saisie.HeureFin);

        if (chevauchement is not null)
        {
            return Conflict(new
            {
                message = $"Cette plage chevauche une plage existante de "
                          + $"{chevauchement.HeureDebut:HH\\:mm} à {chevauchement.HeureFin:HH\\:mm} le même jour."
            });
        }

        var disponibilite = new Disponibilite
        {
            IdUtilisateur = id,
            JourSemaine = saisie.JourSemaine,
            HeureDebut = saisie.HeureDebut,
            HeureFin = saisie.HeureFin
        };

        _contexte.Disponibilites.Add(disponibilite);
        await _contexte.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ListerDisponibilites),
            new { id },
            DisponibiliteDto.Depuis(disponibilite));
    }

    /// <summary>Supprime une plage de disponibilité.</summary>
    [HttpDelete("api/disponibilites/{id:int}")]
    public async Task<IActionResult> SupprimerDisponibilite(int id)
    {
        var disponibilite = await _contexte.Disponibilites.FindAsync(id);

        if (disponibilite is null)
        {
            return NotFound(new { message = $"Aucune disponibilité avec l'identifiant {id}." });
        }

        _contexte.Disponibilites.Remove(disponibilite);
        await _contexte.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Vérifie que l'utilisateur existe et qu'il est bien un agent.
    /// Retourne null si tout est correct, sinon la réponse d'erreur à renvoyer.
    /// </summary>
    private async Task<ActionResult?> ValiderAgent(int id)
    {
        var utilisateur = await _contexte.Utilisateurs.FindAsync(id);

        if (utilisateur is null)
        {
            return NotFound(new { message = $"Aucun utilisateur avec l'identifiant {id}." });
        }

        if (utilisateur.Role != Role.AGENT)
        {
            return BadRequest(new
            {
                message = $"L'utilisateur {id} a le rôle {utilisateur.Role} : seuls les agents ont des disponibilités."
            });
        }

        return null;
    }
}
