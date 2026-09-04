# LocaVisite

Système de gestion des visites de logements locatifs, pour une agence de
location.

Une agence coordonne aujourd'hui ses visites par téléphone, courriel et
fichiers Excel : des agents réservés deux fois pour la même heure, des visites
oubliées, des prospects qui attendent sur place. LocaVisite centralise tout ça.

Un prospect consulte le catalogue et demande une visite en proposant une plage
horaire. Un préposé reçoit la demande, voit quels agents sont réellement libres
pendant cette plage, et en assigne un.

## Portée du dépôt

| Composant | État |
|---|---|
| API REST (ASP.NET Core) | authentification, logements, demandes de visite, disponibilités, recherche d'agents, assignation |
| Site Web public (React) | catalogue filtrable, fiche de logement, demande de visite |
| Interface de gestion (React) | branche `partie-5-web-gestion`, pas encore fusionnée |
| Application mobile des agents | à venir |

## Pile technique

| Couche | Technologie |
|---|---|
| API | ASP.NET Core 9, contrôleurs |
| ORM | Entity Framework Core 9, migrations |
| Base de données | SQL Server LocalDB |
| Authentification | JWT (jeton porteur), BCrypt |
| Web | React 19, Vite, React Router, axios |
| Tests | xUnit, EF Core InMemory |

## Prérequis

- [.NET SDK 9](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- SQL Server LocalDB (installé avec Visual Studio, ou via le SQL Server Express
  Installer)

## Démarrage

### 1. L'API

La clé de signature des jetons n'est pas versionnée. Il faut la définir une
fois, localement :

```bash
cd src/LocaVisite.Api
dotnet user-secrets set "Jwt:Cle" "une-cle-d-au-moins-32-caracteres"
dotnet run
```

L'API écoute sur `http://localhost:5129`. La base est créée, migrée et remplie
de données de départ au premier démarrage — aucune commande `dotnet ef` à
lancer à la main.

Swagger s'ouvre à la racine : <http://localhost:5129>

### 2. Le site Web

Dans un second terminal, l'API devant déjà tourner :

```bash
cd src/locavisite-web
npm install
npm run dev
```

Le site s'ouvre sur <http://localhost:5173>.

L'adresse de l'API se configure dans `.env.development` (`VITE_API_URL`). Cette
origine doit aussi figurer dans `OriginesAutorisees` de `appsettings.json`,
sinon le navigateur bloque les appels.

## Comptes de démonstration

Créés automatiquement par le semeur au premier démarrage.

| Rôle | Courriel | Mot de passe |
|---|---|---|
| Préposé | `prepose@locavisite.ca` | `Test1234!` |
| Agent | `agent@locavisite.ca` | `Test1234!` |

## Tests

```bash
dotnet test
```

Quinze tests, dont les neuf qui couvrent la recherche d'agents disponibles et
six qui valident la plage horaire soumise par un prospect.

## Structure

```
src/LocaVisite.Api/       API REST
  Models/                 les huit entités du modèle de données
  Data/                   DbContext, migrations, semeur
  Services/               logique métier
  Controllers/            points d'entrée HTTP
  Dtos/                   contrats d'entrée et de sortie
src/locavisite-web/       application React
  src/api/                client axios et appels
  src/pages/              une page par route
  src/components/         éléments réutilisables
tests/LocaVisite.Tests/   tests xUnit
```

## La règle métier centrale

La recherche d'agents disponibles est la partie non triviale du projet.
`GET /api/visites/{id}/agents-disponibles` ne retourne pas une liste d'agents
mais une liste de **créneaux** — des couples *(agent, heure de début)* :

```json
{
  "dureeRequise": 30,
  "plageDemandee": { "date": "2026-09-15", "debut": "14:00", "fin": "16:00" },
  "creneaux": [
    { "idAgent": 2, "nomAgent": "Luc Gagnon", "heureDebut": "14:00", "heureFin": "14:30" }
  ],
  "raison": null
}
```

Les règles appliquées :

1. Le résultat est une liste de créneaux, pas d'agents : le préposé a besoin de
   l'heure à retenir.
2. Un tampon fixe de **30 minutes** est exigé entre deux visites d'un même
   agent, avant comme après. Le temps de déplacement réel n'est pas calculé.
3. Les heures de début candidates vont de quart d'heure en quart d'heure.
4. Toute visite qui n'est pas `ANNULEE` occupe l'agent.
5. Quand aucun créneau ne convient, la réponse porte une **raison** en clair.
   Sans elle, le préposé conclurait à tort que tous les agents sont occupés.

Au moment d'assigner, le créneau est **revalidé** côté serveur : il a pu être
pris entre l'affichage de la liste et le clic du préposé.

## Cycle de vie d'une visite

```
DEMANDEE → ASSIGNEE → EN_ROUTE → EN_COURS → TERMINEE
```

Une visite peut être annulée depuis `DEMANDEE` ou `ASSIGNEE`. Aucune autre
transition n'est permise.

## Conventions

- Le domaine est en français : entités, propriétés, routes et commentaires.
- Les tables et colonnes suivent le modèle logique remis : tables en
  MAJUSCULES, colonnes en `snake_case`, mappées par `[Table]` et `[Column]`.
- Les logements ne sont jamais supprimés : leur statut passe à `RETIRE`.
- Aucun secret dans le dépôt : la clé JWT passe par `dotnet user-secrets`.

---

Projet réalisé dans le cadre d'un cours de développement d'applications.
Travail individuel.
