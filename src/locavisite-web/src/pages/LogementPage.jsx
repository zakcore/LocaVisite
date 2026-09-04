import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { obtenirLogement } from '../api/logements'
import { messageErreur } from '../api/client'
import { formaterLoyer } from '../format'
import Chargement from '../components/Chargement'
import MessageErreur from '../components/MessageErreur'
import Introuvable from '../components/Introuvable'

export default function LogementPage() {
  const { id } = useParams()
  const [logement, setLogement] = useState(null)
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState(null)
  const [introuvable, setIntrouvable] = useState(false)

  useEffect(() => {
    let actif = true

    async function charger() {
      setChargement(true)
      setErreur(null)
      setIntrouvable(false)

      try {
        const donnees = await obtenirLogement(id)
        if (actif) setLogement(donnees)
      } catch (probleme) {
        if (!actif) return

        // Un identifiant inconnu n'est pas une panne : c'est une page
        // « introuvable », pas un message d'erreur technique.
        if (probleme.response?.status === 404) {
          setIntrouvable(true)
        } else {
          setErreur(messageErreur(probleme))
        }
      } finally {
        if (actif) setChargement(false)
      }
    }

    charger()
    return () => { actif = false }
  }, [id])

  if (chargement) return <Chargement texte="Chargement du logement…" />
  if (introuvable) return <Introuvable />
  if (erreur) return <MessageErreur message={erreur} />
  if (!logement) return null

  return (
    <article className="fiche">
      <p className="fiche__ville">{logement.ville}</p>
      <h1>{logement.adresse}</h1>
      <p className="fiche__loyer">{formaterLoyer(logement.loyerMensuel)}</p>

      <dl className="fiche__caracteristiques">
        <div><dt>Type</dt><dd>{logement.type}</dd></div>
        <div><dt>Pièces</dt><dd>{logement.nbPieces}</dd></div>
        <div><dt>Code postal</dt><dd>{logement.codePostal}</dd></div>
        <div><dt>Statut</dt><dd>{logement.statut}</dd></div>
      </dl>

      {logement.description && (
        <section className="fiche__description">
          <h2>Description</h2>
          <p>{logement.description}</p>
        </section>
      )}

      <div className="fiche__actions">
        <Link to={`/logements/${logement.idLogement}/demande`} className="bouton">
          Demander une visite
        </Link>
        <Link to="/" className="bouton bouton--secondaire">Retour au catalogue</Link>
      </div>
    </article>
  )
}
