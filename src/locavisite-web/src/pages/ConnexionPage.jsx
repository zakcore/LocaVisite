import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/ContexteAuth'
import { messageErreur } from '../api/client'
import MessageErreur from '../components/MessageErreur'

export default function ConnexionPage() {
  const { connexion } = useAuth()
  const navigate = useNavigate()
  const emplacement = useLocation()

  const [courriel, setCourriel] = useState('')
  const [motDePasse, setMotDePasse] = useState('')
  const [erreur, setErreur] = useState(null)
  const [envoi, setEnvoi] = useState(false)

  async function soumettre(evenement) {
    evenement.preventDefault()
    setErreur(null)
    setEnvoi(true)

    try {
      await connexion(courriel, motDePasse)
      // Retour a la page demandee avant la redirection, sinon la gestion.
      navigate(emplacement.state?.depuis ?? '/gestion/visites', { replace: true })
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <section className="formulaire formulaire--etroit">
      <h1>Connexion</h1>
      <p className="formulaire__logement">Réservé au personnel de l'agence.</p>

      <form onSubmit={soumettre} noValidate>
        <label className="champ">
          <span>Courriel *</span>
          <input type="email" value={courriel} autoComplete="username"
            onChange={(e) => setCourriel(e.target.value)} required />
        </label>

        <label className="champ" style={{ marginTop: '0.9rem' }}>
          <span>Mot de passe *</span>
          <input type="password" value={motDePasse} autoComplete="current-password"
            onChange={(e) => setMotDePasse(e.target.value)} required />
        </label>

        {erreur && <div style={{ marginTop: '1rem' }}><MessageErreur message={erreur} /></div>}

        <div className="fiche__actions">
          <button type="submit" className="bouton" disabled={envoi}>
            {envoi ? 'Connexion…' : 'Se connecter'}
          </button>
        </div>
      </form>
    </section>
  )
}
