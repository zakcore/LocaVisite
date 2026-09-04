import { Link, Route, Routes } from 'react-router-dom'
import CataloguePage from './pages/CataloguePage'
import LogementPage from './pages/LogementPage'
import DemandeVisitePage from './pages/DemandeVisitePage'
import Introuvable from './components/Introuvable'

export default function App() {
  return (
    <div className="application">
      <header className="entete">
        <div className="entete__contenu">
          <Link to="/" className="entete__marque">LocaVisite</Link>
          <nav>
            <Link to="/" className="entete__lien">Catalogue</Link>
          </nav>
        </div>
      </header>

      <main className="contenu">
        <Routes>
          <Route path="/" element={<CataloguePage />} />
          <Route path="/logements/:id" element={<LogementPage />} />
          <Route path="/logements/:id/demande" element={<DemandeVisitePage />} />
          <Route path="*" element={<Introuvable />} />
        </Routes>
      </main>

      <footer className="pied">
        <p>LocaVisite — gestion des visites de logements locatifs</p>
      </footer>
    </div>
  )
}
