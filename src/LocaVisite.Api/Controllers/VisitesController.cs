using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using LocaVisite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Controllers;

[ApiController]
[Route("api/visites")]
public class VisitesController : ControllerBase
{
    private readonly LocaVisiteContext _contexte;
    private readonly ServiceRechercheAgents _rechercheAgents;

    public VisitesController(LocaVisiteContext contexte, ServiceRechercheAgents rechercheAgents)
    {
        _contexte = contexte;
        _rechercheAgents = rechercheAgents;
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

        // Une plage plus courte qu'une visite ne pourra jamais être assignée :
        // la recherche d'agents la rejetterait toujours. Autant la refuser
        // tout de suite, pendant que le prospect peut encore la corriger.
        var minutesPlage = (int)(demande.HeureSouhaiteeFin - demande.HeureSouhaiteeDebut).TotalMinutes;

        if (minutesPlage < Visite.DureeParDefautMinutes)
        {
            return BadRequest(new
            {
                message = $"La plage proposée ne dure que {minutesPlage} minutes. "
                          + $"Une visite dure au moins {Visite.DureeParDefautMinutes} minutes : "
                          + "proposez une plage plus large."
            });
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
            DureePrevue = Visite.DureeParDefautMinutes,
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

    /// <summary>
    /// Créneaux (agent, heure de début) qui conviennent à cette visite.
    /// Le détail de l'algorithme est dans <see cref="ServiceRechercheAgents"/>.
    /// </summary>
    [HttpGet("{id:int}/agents-disponibles")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<AgentsDisponiblesDto>> AgentsDisponibles(int id)
    {
        var visite = await _contexte.Visites.FindAsync(id);

        if (visite is null)
        {
            return NotFound(new { message = $"Aucune visite avec l'identifiant {id}." });
        }

        if (visite.Statut != StatutVisite.DEMANDEE)
        {
            return Conflict(new
            {
                message = $"Cette visite est au statut {visite.Statut} : "
                          + "la recherche d'agents ne s'applique qu'aux visites DEMANDEE."
            });
        }

        return Ok(await _rechercheAgents.ChercherAsync(visite));
    }

    /// <summary>
    /// Assigne un agent à un créneau et fait passer la visite à ASSIGNEE.
    /// </summary>
    [HttpPost("{id:int}/assigner")]
    [Authorize(Roles = "PREPOSE,GESTIONNAIRE")]
    public async Task<ActionResult<VisiteDto>> Assigner(int id, [FromBody] AssignationDto saisie)
    {
        var visite = await _contexte.Visites
            .Include(v => v.Logement)
            .Include(v => v.Prospect)
            .FirstOrDefaultAsync(v => v.IdVisite == id);

        if (visite is null)
        {
            return NotFound(new { message = $"Aucune visite avec l'identifiant {id}." });
        }

        if (visite.Statut != StatutVisite.DEMANDEE)
        {
            return Conflict(new
            {
                message = $"Cette visite est au statut {visite.Statut} : "
                          + "seule une visite DEMANDEE peut être assignée."
            });
        }

        var agent = await _contexte.Utilisateurs.FindAsync(saisie.IdAgent);

        if (agent is null)
        {
            return NotFound(new { message = $"Aucun utilisateur avec l'identifiant {saisie.IdAgent}." });
        }

        if (agent.Role != Role.AGENT)
        {
            return BadRequest(new
            {
                message = $"L'utilisateur {agent.IdUtilisateur} a le rôle {agent.Role} : "
                          + "seul un agent peut se voir assigner une visite."
            });
        }

        // Le créneau affiché au préposé a pu être pris entre-temps par un autre
        // préposé. On ne fait donc pas confiance à ce que le client envoie : on
        // refait la recherche et on vérifie que le créneau demandé y figure
        // toujours.
        var heureDemandee = saisie.HeureDebut.ToString("HH\\:mm");
        var creneaux = await _rechercheAgents.ChercherTousLesCreneauxAsync(visite);

        var creneauRetenu = creneaux.FirstOrDefault(c =>
            c.IdAgent == saisie.IdAgent && c.HeureDebut == heureDemandee);

        if (creneauRetenu is null)
        {
            return Conflict(new
            {
                message = $"Le créneau de {heureDemandee} n'est plus disponible pour "
                          + $"{agent.Prenom} {agent.Nom}. Rafraîchissez la liste des agents disponibles."
            });
        }

        visite.IdAgent = agent.IdUtilisateur;
        visite.DatePrevue = visite.DateSouhaitee;
        visite.HeurePrevue = saisie.HeureDebut;
        visite.Statut = StatutVisite.ASSIGNEE;

        await _contexte.SaveChangesAsync();

        return Ok(VisiteDto.Depuis(visite));
    }
}
