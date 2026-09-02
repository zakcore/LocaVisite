using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Controllers;

[ApiController]
[Route("api/visites")]
public class VisitesController : ControllerBase
{
    private readonly LocaVisiteContext _contexte;

    public VisitesController(LocaVisiteContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>
    /// Demande de visite déposée par un prospect depuis le site public.
    /// Crée le prospect s'il est inconnu, puis la visite à l'état DEMANDEE.
    /// </summary>
    [HttpPost("demande")]
    [AllowAnonymous]
    public async Task<ActionResult<ReponseDemandeVisiteDto>> Demander([FromBody] DemandeVisiteDto demande)
    {
        if (demande.HeureSouhaiteeFin <= demande.HeureSouhaiteeDebut)
        {
            return BadRequest(new { message = "L'heure de fin doit être postérieure à l'heure de début." });
        }

        if (demande.DateSouhaitee < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "La date souhaitée ne peut pas être dans le passé." });
        }

        var logement = await _contexte.Logements.FindAsync(demande.IdLogement);

        if (logement is null)
        {
            return NotFound(new { message = $"Aucun logement avec l'identifiant {demande.IdLogement}." });
        }

        if (logement.Statut != StatutLogement.DISPONIBLE)
        {
            return Conflict(new
            {
                message = $"Le logement {logement.IdLogement} n'est pas offert en visite : son statut est {logement.Statut}."
            });
        }

        // Un même prospect peut demander plusieurs visites : on le retrouve par
        // son courriel plutôt que d'en créer un doublon à chaque demande.
        var prospect = await _contexte.Prospects
            .FirstOrDefaultAsync(p => p.Courriel == demande.Courriel);

        if (prospect is null)
        {
            prospect = new Prospect
            {
                Nom = demande.Nom,
                Prenom = demande.Prenom,
                Courriel = demande.Courriel,
                Telephone = demande.Telephone,
                PossedeMobile = demande.PossedeMobile,
                DateCreation = DateTime.Now
            };

            _contexte.Prospects.Add(prospect);
        }
        else
        {
            // Les coordonnées les plus récentes sont les bonnes : c'est par elles
            // que le prospect sera joint pour cette visite-ci.
            prospect.Telephone = demande.Telephone;
            prospect.PossedeMobile = demande.PossedeMobile;
        }

        var visite = new Visite
        {
            IdLogement = logement.IdLogement,
            Prospect = prospect,
            IdAgent = null,
            DateSouhaitee = demande.DateSouhaitee,
            HeureSouhaiteeDebut = demande.HeureSouhaiteeDebut,
            HeureSouhaiteeFin = demande.HeureSouhaiteeFin,
            DureePrevue = 30,
            Statut = StatutVisite.DEMANDEE
        };

        _contexte.Visites.Add(visite);
        await _contexte.SaveChangesAsync();

        return Ok(new ReponseDemandeVisiteDto
        {
            IdVisite = visite.IdVisite,
            IdProspect = prospect.IdProspect,
            Statut = visite.Statut.ToString(),
            Message = "Votre demande de visite a bien été reçue. Un préposé communiquera avec vous."
        });
    }

    /// <summary>Liste des visites, filtrable par statut et par date.</summary>
    [HttpGet]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<IEnumerable<VisiteDto>>> Lister(
        [FromQuery] string? statut,
        [FromQuery] DateOnly? date)
    {
        var requete = _contexte.Visites
            .Include(v => v.Logement)
            .Include(v => v.Prospect)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statut))
        {
            if (!Enum.TryParse<StatutVisite>(statut, ignoreCase: true, out var statutRecherche))
            {
                return BadRequest(new
                {
                    message = $"Statut « {statut} » inconnu. Valeurs acceptées : {string.Join(", ", Enum.GetNames<StatutVisite>())}."
                });
            }

            requete = requete.Where(v => v.Statut == statutRecherche);
        }

        if (date.HasValue)
        {
            // Une visite assignée se cherche par sa date retenue ; une visite
            // encore DEMANDEE n'en a pas, on retombe alors sur la date souhaitée.
            requete = requete.Where(v => (v.DatePrevue ?? v.DateSouhaitee) == date.Value);
        }

        var visites = await requete
            .OrderBy(v => v.DatePrevue ?? v.DateSouhaitee)
            .ThenBy(v => v.HeurePrevue ?? v.HeureSouhaiteeDebut)
            .ToListAsync();

        return Ok(visites.Select(VisiteDto.Depuis));
    }

    /// <summary>Détail d'une visite, avec son logement et son prospect.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<VisiteDto>> Obtenir(int id)
    {
        var visite = await _contexte.Visites
            .Include(v => v.Logement)
            .Include(v => v.Prospect)
            .FirstOrDefaultAsync(v => v.IdVisite == id);

        if (visite is null)
        {
            return NotFound(new { message = $"Aucune visite avec l'identifiant {id}." });
        }

        return Ok(VisiteDto.Depuis(visite));
    }

    /// <summary>
    /// Annule une visite. Seules les transitions DEMANDEE → ANNULEE et
    /// ASSIGNEE → ANNULEE existent dans le cycle de vie.
    /// </summary>
    [HttpPost("{id:int}/annuler")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<VisiteDto>> Annuler(int id)
    {
        var visite = await _contexte.Visites
            .Include(v => v.Logement)
            .Include(v => v.Prospect)
            .FirstOrDefaultAsync(v => v.IdVisite == id);

        if (visite is null)
        {
            return NotFound(new { message = $"Aucune visite avec l'identifiant {id}." });
        }

        if (visite.Statut != StatutVisite.DEMANDEE && visite.Statut != StatutVisite.ASSIGNEE)
        {
            return Conflict(new
            {
                message = $"Une visite au statut {visite.Statut} ne peut pas être annulée. "
                          + "Seules les visites DEMANDEE ou ASSIGNEE peuvent l'être."
            });
        }

        visite.Statut = StatutVisite.ANNULEE;
        await _contexte.SaveChangesAsync();

        return Ok(VisiteDto.Depuis(visite));
    }

    /// <summary>Fixe la durée prévue de la visite, en minutes, avant la recherche d'un agent.</summary>
    [HttpPut("{id:int}/duree")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<VisiteDto>> FixerDuree(int id, [FromBody] DureeVisiteDto saisie)
    {
        if (saisie.DureePrevue % 15 != 0)
        {
            return BadRequest(new { message = "La durée doit être un multiple de 15 minutes." });
        }

        var visite = await _contexte.Visites
            .Include(v => v.Logement)
            .Include(v => v.Prospect)
            .FirstOrDefaultAsync(v => v.IdVisite == id);

        if (visite is null)
        {
            return NotFound(new { message = $"Aucune visite avec l'identifiant {id}." });
        }

        // Changer la durée d'une visite déjà commencée ou terminée fausserait
        // l'horaire de l'agent : on ne l'autorise qu'avant le départ.
        if (visite.Statut != StatutVisite.DEMANDEE && visite.Statut != StatutVisite.ASSIGNEE)
        {
            return Conflict(new
            {
                message = $"La durée d'une visite au statut {visite.Statut} ne peut plus être modifiée."
            });
        }

        visite.DureePrevue = saisie.DureePrevue;
        await _contexte.SaveChangesAsync();

        return Ok(VisiteDto.Depuis(visite));
    }
}
