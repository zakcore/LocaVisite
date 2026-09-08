import client from './client'

export async function listerAgents() {
  const reponse = await client.get('/api/agents')
  return reponse.data
}

export async function listerDisponibilites(idAgent) {
  const reponse = await client.get(`/api/agents/${idAgent}/disponibilites`)
  return reponse.data
}

export async function ajouterDisponibilite(idAgent, plage) {
  const reponse = await client.post(`/api/agents/${idAgent}/disponibilites`, plage)
  return reponse.data
}

export async function supprimerDisponibilite(idDisponibilite) {
  await client.delete(`/api/disponibilites/${idDisponibilite}`)
}
