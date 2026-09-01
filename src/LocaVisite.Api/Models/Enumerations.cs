namespace LocaVisite.Api.Models;

/// <summary>Rôle d'un utilisateur du système.</summary>
public enum Role
{
    PREPOSE,
    AGENT,
    GESTIONNAIRE
}

/// <summary>Fournisseur ayant authentifié l'utilisateur.</summary>
public enum FournisseurAuth
{
    LOCAL,
    GOOGLE,
    FACEBOOK
}

/// <summary>Statut d'un logement au catalogue.</summary>
public enum StatutLogement
{
    DISPONIBLE,
    RESERVE,
    LOUE,
    RETIRE
}

/// <summary>Étape du cycle de vie d'une visite.</summary>
public enum StatutVisite
{
    DEMANDEE,
    ASSIGNEE,
    EN_ROUTE,
    EN_COURS,
    TERMINEE,
    ANNULEE
}

/// <summary>Intérêt manifesté par le prospect lors de la visite.</summary>
public enum NiveauInteret
{
    FAIBLE,
    MOYEN,
    ELEVE
}

/// <summary>Canal utilisé pour joindre un prospect.</summary>
public enum TypeCommunication
{
    SMS,
    APPEL
}
