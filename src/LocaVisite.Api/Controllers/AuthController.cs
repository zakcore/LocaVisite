using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LocaVisiteContext _contexte;
    private readonly ServiceJeton _serviceJeton;

    public AuthController(LocaVisiteContext contexte, ServiceJeton serviceJeton)
    {
        _contexte = contexte;
        _serviceJeton = serviceJeton;
    }

    /// <summary>Vérifie les identifiants et retourne un jeton porteur.</summary>
    [HttpPost("connexion")]
    [AllowAnonymous]
    public async Task<ActionResult<ReponseConnexionDto>> Connexion([FromBody] ConnexionDto saisie)
    {
        var utilisateur = await _contexte.Utilisateurs
            .FirstOrDefaultAsync(u => u.Courriel == saisie.Courriel);

        // Le même message dans les deux cas : révéler qu'un courriel existe
        // permettrait d'énumérer les comptes.
        const string messageErreur = "Courriel ou mot de passe invalide.";

        if (utilisateur is null || string.IsNullOrEmpty(utilisateur.MotDePasse))
        {
            return Unauthorized(new { message = messageErreur });
        }

        if (!BCrypt.Net.BCrypt.Verify(saisie.MotDePasse, utilisateur.MotDePasse))
        {
            return Unauthorized(new { message = messageErreur });
        }

        return Ok(new ReponseConnexionDto
        {
            Jeton = _serviceJeton.GenererJeton(utilisateur),
            Nom = $"{utilisateur.Prenom} {utilisateur.Nom}",
            Role = utilisateur.Role.ToString()
        });
    }
}
