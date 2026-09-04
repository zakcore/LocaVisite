import axios from 'axios'

// L'URL de l'API n'est pas codee en dur : elle vient de .env.development
// (variable VITE_API_URL), pour pouvoir viser un autre serveur sans toucher
// au code.
const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
})

// La partie publique n'a pas besoin de jeton ; l'interface de gestion en a un.
// L'intercepteur le joint des qu'il existe, et ne fait rien sinon.
client.interceptors.request.use((config) => {
  const jeton = lireJeton()

  if (jeton) {
    config.headers.Authorization = `Bearer ${jeton}`
  }

  return config
})

/** Le jeton vit dans sessionStorage : il disparait a la fermeture du navigateur. */
function lireJeton() {
  try {
    const session = sessionStorage.getItem('locavisite.session')
    return session ? JSON.parse(session).jeton : null
  } catch {
    return null
  }
}

/**
 * Traduit une erreur axios en message lisible.
 *
 * L'API renvoie des messages explicites — « Le logement 2 n'est pas offert en
 * visite », « L'heure de fin doit etre posterieure a l'heure de debut » — et
 * c'est ceux-la qu'il faut montrer, pas un « une erreur est survenue »
 * generique qui n'aide personne.
 */
export function messageErreur(erreur) {
  const donnees = erreur.response?.data

  if (donnees?.message) {
    return donnees.message
  }

  // Erreurs de validation ASP.NET : { errors: { champ: ["..."] } }
  if (donnees?.errors) {
    const premier = Object.values(donnees.errors).flat()[0]

    if (premier) {
      return premier
    }
  }

  if (donnees?.title) {
    return donnees.title
  }

  // Pas de reponse du tout : l'API est arretee ou injoignable.
  if (erreur.request) {
    return "Impossible de joindre le serveur. Vérifiez que l'API est démarrée."
  }

  return 'Une erreur inattendue est survenue.'
}

export default client
