import client from './client'

/**
 * Catalogue public.
 *
 * Les filtres partent en parametres de requete : c'est l'API qui filtre,
 * pas le navigateur. Les champs vides sont retires pour ne pas envoyer
 * « ville= » a vide.
 */
export async function listerLogements(filtres = {}) {
  const params = {}

  if (filtres.ville?.trim()) params.ville = filtres.ville.trim()
  if (filtres.loyerMax) params.loyerMax = filtres.loyerMax
  if (filtres.nbPiecesMin) params.nbPiecesMin = filtres.nbPiecesMin
  if (filtres.type?.trim()) params.type = filtres.type.trim()

  const reponse = await client.get('/api/logements', { params })
  return reponse.data
}

export async function obtenirLogement(id) {
  const reponse = await client.get(`/api/logements/${id}`)
  return reponse.data
}

/** Vue de gestion : tous les logements, y compris les RETIRE. */
export async function listerTousLesLogements() {
  const reponse = await client.get('/api/logements/tous')
  return reponse.data
}

export async function creerLogement(logement) {
  const reponse = await client.post('/api/logements', logement)
  return reponse.data
}

export async function modifierLogement(id, logement) {
  const reponse = await client.put(`/api/logements/${id}`, logement)
  return reponse.data
}

/** Retrait logique : le logement passe au statut RETIRE. */
export async function retirerLogement(id) {
  await client.delete(`/api/logements/${id}`)
}
