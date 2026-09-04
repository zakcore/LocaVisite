import client from './client'

/** Demande de visite deposee par un prospect, sans authentification. */
export async function demanderVisite(demande) {
  const reponse = await client.post('/api/visites/demande', demande)
  return reponse.data
}

export async function listerVisites(filtres = {}) {
  const params = {}
  if (filtres.statut) params.statut = filtres.statut
  if (filtres.date) params.date = filtres.date

  const reponse = await client.get('/api/visites', { params })
  return reponse.data
}

export async function obtenirVisite(id) {
  const reponse = await client.get(`/api/visites/${id}`)
  return reponse.data
}

export async function annulerVisite(id) {
  const reponse = await client.post(`/api/visites/${id}/annuler`)
  return reponse.data
}

export async function fixerDuree(id, dureePrevue) {
  const reponse = await client.put(`/api/visites/${id}/duree`, { dureePrevue })
  return reponse.data
}

export async function chercherAgentsDisponibles(id) {
  const reponse = await client.get(`/api/visites/${id}/agents-disponibles`)
  return reponse.data
}

export async function assignerVisite(id, idAgent, heureDebut) {
  const reponse = await client.post(`/api/visites/${id}/assigner`, { idAgent, heureDebut })
  return reponse.data
}
