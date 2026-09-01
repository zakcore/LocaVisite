using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Controllers;

[ApiController]
[Route("api/logements")]
public class LogementsController : ControllerBase
{
    private readonly LocaVisiteContext _contexte;

    public LogementsController(LocaVisiteContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>Catalogue public. Ne retourne que les logements DISPONIBLE.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LogementDto>>> Lister(
        [FromQuery] string? ville,
        [FromQuery] decimal? loyerMax,
        [FromQuery] int? nbPiecesMin,
        [FromQuery] string? type)
    {
        var requete = _contexte.Logements
            .Where(l => l.Statut == StatutLogement.DISPONIBLE);

        if (!string.IsNullOrWhiteSpace(ville))
        {
            requete = requete.Where(l => l.Ville == ville);
        }

        if (loyerMax.HasValue)
        {
            requete = requete.Where(l => l.LoyerMensuel <= loyerMax.Value);
        }

        if (nbPiecesMin.HasValue)
        {
            requete = requete.Where(l => l.NbPieces >= nbPiecesMin.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            requete = requete.Where(l => l.Type == type);
        }

        var logements = await requete
            .OrderBy(l => l.LoyerMensuel)
            .ToListAsync();

        return Ok(logements.Select(LogementDto.Depuis));
    }

    /// <summary>Fiche d'un logement.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<LogementDto>> Obtenir(int id)
    {
        var logement = await _contexte.Logements.FindAsync(id);

        if (logement is null)
        {
            return NotFound(new { message = $"Aucun logement avec l'identifiant {id}." });
        }

        return Ok(LogementDto.Depuis(logement));
    }

    [HttpPost]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<LogementDto>> Creer([FromBody] LogementSaisieDto saisie)
    {
        var logement = new Logement
        {
            Adresse = saisie.Adresse,
            Ville = saisie.Ville,
            CodePostal = saisie.CodePostal,
            Type = saisie.Type,
            NbPieces = saisie.NbPieces,
            LoyerMensuel = saisie.LoyerMensuel,
            Description = saisie.Description,
            Statut = saisie.Statut ?? StatutLogement.DISPONIBLE,
            Latitude = saisie.Latitude,
            Longitude = saisie.Longitude
        };

        _contexte.Logements.Add(logement);
        await _contexte.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtenir), new { id = logement.IdLogement }, LogementDto.Depuis(logement));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<LogementDto>> Modifier(int id, [FromBody] LogementSaisieDto saisie)
    {
        var logement = await _contexte.Logements.FindAsync(id);

        if (logement is null)
        {
            return NotFound(new { message = $"Aucun logement avec l'identifiant {id}." });
        }

        logement.Adresse = saisie.Adresse;
        logement.Ville = saisie.Ville;
        logement.CodePostal = saisie.CodePostal;
        logement.Type = saisie.Type;
        logement.NbPieces = saisie.NbPieces;
        logement.LoyerMensuel = saisie.LoyerMensuel;
        logement.Description = saisie.Description;
        logement.Latitude = saisie.Latitude;
        logement.Longitude = saisie.Longitude;

        if (saisie.Statut.HasValue)
        {
            logement.Statut = saisie.Statut.Value;
        }

        await _contexte.SaveChangesAsync();

        return Ok(LogementDto.Depuis(logement));
    }

    /// <summary>Retrait logique : le logement passe au statut RETIRE, il n'est jamais supprimé.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<IActionResult> Retirer(int id)
    {
        var logement = await _contexte.Logements.FindAsync(id);

        if (logement is null)
        {
            return NotFound(new { message = $"Aucun logement avec l'identifiant {id}." });
        }

        if (logement.Statut == StatutLogement.RETIRE)
        {
            return Conflict(new { message = "Ce logement est déjà retiré." });
        }

        logement.Statut = StatutLogement.RETIRE;
        await _contexte.SaveChangesAsync();

        return NoContent();
    }
}
