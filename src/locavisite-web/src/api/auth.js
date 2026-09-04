import client from './client'

export async function connexionApi(courriel, motDePasse) {
  const reponse = await client.post('/api/auth/connexion', { courriel, motDePasse })
  return reponse.data
}
