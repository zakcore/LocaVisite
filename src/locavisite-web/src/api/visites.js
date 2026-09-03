import client from './client'

/** Demande de visite deposee par un prospect, sans authentification. */
export async function demanderVisite(demande) {
  const reponse = await client.post('/api/visites/demande', demande)
  return reponse.data
}
