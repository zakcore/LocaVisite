import { Link, NavLink, Route, Routes, useNavigate } from 'react-router-dom'
import CataloguePage from './pages/CataloguePage'
import LogementPage from './pages/LogementPage'
import DemandeVisitePage from './pages/DemandeVisitePage'
import ConnexionPage from './pages/ConnexionPage'
import LogementsGestionPage from './pages/gestion/LogementsGestionPage'
import VisitesGestionPage from './pages/gestion/VisitesGestionPage'
import VisiteDetailPage from './pages/gestion/VisiteDetailPage'
import AgentsGestionPage from './pages/gestion/AgentsGestionPage'
import Introuvable from './components/Introuvable'
import RouteProtegee from './auth/RouteProtegee'
import { useAuth } from './auth/ContexteAuth'

function Entete() {
  const { session, estConnecte, deconnexion } = useAuth()
  const navigate = useNavigate()

  function seDeconnecter() {
    deconnexion()
    navigate('/connexion', { replace: true })
  }

  return (
    <header className="entete">
      <div className="entete__contenu">
        <Link to="/" className="entete__marque">LocaVisite</Link>

        <nav className="entete__nav">
          <NavLink to="/" end className="entete__lien">Catalogue</NavLink>

          {estConnecte && (
            <>
              <NavLink to="/gestion/visites" className="entete__lien">Visites</NavLink>
              <NavLink to="/gestion/logements" className="entete__lien">Logements</NavLink>
              <NavLink to="/gestion/agents" className="entete__lien">Agents</NavLink>
            </>
          )}
        </nav>

        <div className="entete__session">
          {estConnecte ? (
            <>
              <span className="entete__utilisateur">
                {session.nom} · {session.role}
              </span>
              <button type="button" className="bouton bouton--petit bouton--clair"
                onClick={seDeconnecter}>
                Déconnexion
              </button>
            </>
          ) : (
            <NavLink to="/connexion" className="entete__lien">Connexion</NavLink>
          )}
        </div>
      </div>
    </header>
  )
}

export default function App() {
  return (
    <div className="application">
      <Entete />

      <main className="contenu">
        <Routes>
          {/* Partie publique */}
          <Route path="/" element={<CataloguePage />} />
          <Route path="/logements/:id" element={<LogementPage />} />
          <Route path="/logements/:id/demande" element={<DemandeVisitePage />} />
          <Route path="/connexion" element={<ConnexionPage />} />

          {/* Partie gestion, reservee aux preposes */}
          <Route path="/gestion/logements" element={
            <RouteProtegee><LogementsGestionPage /></RouteProtegee>} />
          <Route path="/gestion/visites" element={
            <RouteProtegee><VisitesGestionPage /></RouteProtegee>} />
          <Route path="/gestion/visites/:id" element={
            <RouteProtegee><VisiteDetailPage /></RouteProtegee>} />
          <Route path="/gestion/agents" element={
            <RouteProtegee><AgentsGestionPage /></RouteProtegee>} />

          <Route path="*" element={<Introuvable />} />
        </Routes>
      </main>

      <footer className="pied">
        <p>LocaVisite — gestion des visites de logements locatifs</p>
      </footer>
    </div>
  )
}
