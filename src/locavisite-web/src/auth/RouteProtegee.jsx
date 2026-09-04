import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from './ContexteAuth'

/** Renvoie vers la connexion tout visiteur non authentifie. */
export default function RouteProtegee({ children }) {
  const { estConnecte } = useAuth()
  const emplacement = useLocation()

  if (!estConnecte) {
    // On retient la page demandee pour y revenir apres la connexion.
    return <Navigate to="/connexion" state={{ depuis: emplacement.pathname }} replace />
  }

  return children
}
