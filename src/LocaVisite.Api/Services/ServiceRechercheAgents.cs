using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Services;

/// <summary>
/// Cherche les créneaux qui conviennent à une visite : à quelle heure, et avec
/// quel agent, la visite peut-elle avoir lieu dans la plage proposée par le
/// prospect.
///
/// C'est la logique métier centrale du projet. Les cinq règles appliquées ici
/// sont celles de SPECIFICATIONS.md, section « Recherche des agents disponibles » ;
/// chaque étape indique en commentaire la règle qu'elle applique.
///
/// Le service ne dépend que du contexte de données : il se teste sans passer
/// par HTTP.
/// </summary>
public class ServiceRechercheAgents
{
    /// <summary>
    /// Règle 2 : tampon fixe entre deux visites d'un même agent, avant comme
    /// après. Le temps de déplacement réel n'est pas calculé — un appel à
    /// Google Maps par créneau candidat serait lent et coûteux.
    /// </summary>
    public const int TamponMinutes = 30;

    /// <summary>Règle 3 : les heures de début candidates vont de quart d'heure en quart d'heure.</summary>
    public const int PasMinutes = 15;

    /// <summary>Nombre maximal de créneaux présentés au préposé.</summary>
    public const int MaxCreneaux = 20;

    private readonly LocaVisiteContext _contexte;

    public ServiceRechercheAgents(LocaVisiteContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>
    /// Recherche destinée à l'affichage : au plus <see cref="MaxCreneaux"/>
    /// créneaux, avec la raison quand la liste est vide.
    /// </summary>
    public async Task<AgentsDisponiblesDto> ChercherAsync(Visite visite)
    {
        var reponse = await ChercherSansPlafondAsync(visite);

        // Étape 7 — au maximum 20 créneaux présentés au préposé.
        reponse.Creneaux = reponse.Creneaux.Take(MaxCreneaux).ToList();

        return reponse;
    }

    /// <summary>
    /// Recherche destinée à la revalidation au moment d'assigner : mêmes règles,
    /// mais sans le plafond de 20 créneaux. Un créneau valide situé au-delà du
    /// vingtième serait sinon refusé à tort.
    /// </summary>
    public async Task<List<CreneauDto>> ChercherTousLesCreneauxAsync(Visite visite)
    {
        var reponse = await ChercherSansPlafondAsync(visite);
        return reponse.Creneaux;
    }

    /// <summary>
    /// Le cœur de l'algorithme, sans le plafond de l'étape 7.
    /// </summary>
    private async Task<AgentsDisponiblesDto> ChercherSansPlafondAsync(Visite visite)
    {
        var reponse = new AgentsDisponiblesDto
        {
            DureeRequise = visite.DureePrevue,
            PlageDemandee = new PlageDemandeeDto
            {
                Date = visite.DateSouhaitee,
                Debut = visite.HeureSouhaiteeDebut.ToString("HH\\:mm"),
                Fin = visite.HeureSouhaiteeFin.ToString("HH\\:mm")
            }
        };

        var plageDebut = EnMinutes(visite.HeureSouhaiteeDebut);
        var plageFin = EnMinutes(visite.HeureSouhaiteeFin);
        var dureeRequise = visite.DureePrevue;

        // Étape 2 — Règle 5 : une plage trop courte retourne une liste vide
        // AVEC sa raison. Sans la raison, le préposé conclurait à tort que
        // tous les agents sont occupés.
        var minutesPlage = plageFin - plageDebut;
        if (minutesPlage < dureeRequise)
        {
            reponse.Raison =
                $"La plage demandée dure {minutesPlage} minutes, la visite en requiert {dureeRequise}.";
            return reponse;
        }

        // Étape 3 — le jour de la semaine détermine quelles disponibilités
        // hebdomadaires s'appliquent.
        var jourSemaine = (int)visite.DateSouhaitee.DayOfWeek;

        // Étape 4 — tous les agents.
        var agents = await _contexte.Utilisateurs
            .Where(u => u.Role == Role.AGENT)
            .ToListAsync();

        var idsAgents = agents.Select(a => a.IdUtilisateur).ToList();

        var disponibilites = await _contexte.Disponibilites
            .Where(d => d.JourSemaine == jourSemaine && idsAgents.Contains(d.IdUtilisateur))
            .ToListAsync();

        if (disponibilites.Count == 0)
        {
            reponse.Raison = "Aucun agent n'a déclaré de disponibilité ce jour-là.";
            return reponse;
        }

        // Règle 4 : toute visite qui n'est pas ANNULEE occupe l'agent.
        // Les visites DEMANDEE ne bloquent personne : leur id_agent est nul
        // par construction, elles sont donc naturellement exclues ici.
        var visitesOccupantes = await _contexte.Visites
            .Where(v => v.DatePrevue == visite.DateSouhaitee
                        && v.Statut != StatutVisite.ANNULEE
                        && v.IdAgent != null
                        && v.HeurePrevue != null)
            .ToListAsync();

        var creneaux = new List<(CreneauDto Creneau, int ChargeAgent)>();

        // Étape 5 — pour chaque agent.
        foreach (var agent in agents)
        {
            var sesDisponibilites = disponibilites
                .Where(d => d.IdUtilisateur == agent.IdUtilisateur)
                .ToList();

            // Étape 5.1 — aucune disponibilité ce jour-là : l'agent est écarté.
            if (sesDisponibilites.Count == 0)
            {
                continue;
            }

            var sesVisites = visitesOccupantes
                .Where(v => v.IdAgent == agent.IdUtilisateur)
                .Select(v => (
                    Debut: EnMinutes(v.HeurePrevue!.Value),
                    Fin: EnMinutes(v.HeurePrevue!.Value) + v.DureePrevue))
                .ToList();

            foreach (var disponibilite in sesDisponibilites)
            {
                // Étape 5.2 — intersection entre la disponibilité de l'agent et
                // la plage souhaitée par le prospect.
                var debutIntersection = Math.Max(EnMinutes(disponibilite.HeureDebut), plageDebut);
                var finIntersection = Math.Min(EnMinutes(disponibilite.HeureFin), plageFin);

                // Intersection vide, ou trop courte pour y loger la visite.
                if (finIntersection - debutIntersection < dureeRequise)
                {
                    continue;
                }

                // Étape 5.4 — Règle 3 : heures de début candidates de quart
                // d'heure en quart d'heure, tant que la visite entre entièrement
                // dans l'intersection.
                for (var debutCandidat = debutIntersection;
                     debutCandidat + dureeRequise <= finIntersection;
                     debutCandidat += PasMinutes)
                {
                    var finCandidat = debutCandidat + dureeRequise;

                    // Étape 5.5 — Règle 2 : on écarte le candidat qui laisse
                    // moins de 30 minutes d'écart avec une visite déjà prévue.
                    if (!EstLibre(debutCandidat, finCandidat, sesVisites))
                    {
                        continue;
                    }

                    creneaux.Add((
                        new CreneauDto
                        {
                            IdAgent = agent.IdUtilisateur,
                            NomAgent = $"{agent.Prenom} {agent.Nom}",
                            HeureDebut = EnTexte(debutCandidat),
                            HeureFin = EnTexte(finCandidat)
                        },
                        sesVisites.Count));
                }
            }
        }

        // Étape 6 — tri par heure de début croissante, puis, à heure égale,
        // par agent ayant le moins de visites ce jour-là.
        reponse.Creneaux = creneaux
            .OrderBy(c => c.Creneau.HeureDebut)
            .ThenBy(c => c.ChargeAgent)
            .Select(c => c.Creneau)
            .ToList();

        if (reponse.Creneaux.Count == 0)
        {
            reponse.Raison = "Aucun agent n'est libre pendant la plage demandée.";
        }

        return reponse;
    }

    /// <summary>
    /// Règle 2 : un candidat est libre s'il ne chevauche aucune visite prévue et
    /// s'il laisse au moins <see cref="TamponMinutes"/> minutes d'écart avec
    /// chacune, avant comme après.
    /// </summary>
    private static bool EstLibre(int debutCandidat, int finCandidat, List<(int Debut, int Fin)> visites)
    {
        foreach (var visite in visites)
        {
            // Chevauchement pur : les deux visites se recoupent dans le temps.
            if (debutCandidat < visite.Fin && visite.Debut < finCandidat)
            {
                return false;
            }

            // Sinon le candidat est soit entièrement avant, soit entièrement
            // après : on mesure l'écart du bon côté.
            var ecart = finCandidat <= visite.Debut
                ? visite.Debut - finCandidat
                : debutCandidat - visite.Fin;

            if (ecart < TamponMinutes)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Les calculs se font en minutes depuis minuit plutôt qu'en TimeOnly :
    /// les additions restent de simples entiers et ne risquent pas de repasser
    /// par minuit.
    /// </summary>
    private static int EnMinutes(TimeOnly heure) => heure.Hour * 60 + heure.Minute;

    private static string EnTexte(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";
}
