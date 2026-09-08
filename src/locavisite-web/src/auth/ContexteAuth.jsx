import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import client from '../api/client'
import { connexionApi } from '../api/auth'

const ContexteAuth = createContext(null)

// sessionStorage plutot que localStorage : sur un poste partage, le jeton ne
// doit pas survivre a la fermeture du navigateur.
const CLE = 'locavisite.session'

function lireSession() {
  try {
    const brut = sessionStorage.getItem(CLE)
    return brut ? JSON.parse(brut) : null
  } catch {
    return null
  }
}

export function FournisseurAuth({ children }) {
  const [session, setSession] = useState(lireSession)

  const deconnexion = useCallback(() => {
    sessionStorage.removeItem(CLE)
    setSession(null)
  }, [])

  const connexion = useCallback(async (courriel, motDePasse) => {
    const donnees = await connexionApi(courriel, motDePasse)
    sessionStorage.setItem(CLE, JSON.stringify(donnees))
    setSession(donnees)
    return donnees
  }, [])

  // Un jeton expire ou refuse doit deconnecter, sinon l'interface reste
  // affichee alors que plus aucun appel n'aboutit.
  useEffect(() => {
    const intercepteur = client.interceptors.response.use(
      (reponse) => reponse,
      (erreur) => {
        if (erreur.response?.status === 401 && lireSession()) {
          deconnexion()
        }
        return Promise.reject(erreur)
      },
    )

    return () => client.interceptors.response.eject(intercepteur)
  }, [deconnexion])

  const valeur = useMemo(
    () => ({ session, connexion, deconnexion, estConnecte: !!session }),
    [session, connexion, deconnexion],
  )

  return <ContexteAuth.Provider value={valeur}>{children}</ContexteAuth.Provider>
}

export function useAuth() {
  const contexte = useContext(ContexteAuth)

  if (!contexte) {
    throw new Error("useAuth doit etre utilise a l'interieur de FournisseurAuth")
  }

  return contexte
}

export { CLE as CLE_SESSION }
