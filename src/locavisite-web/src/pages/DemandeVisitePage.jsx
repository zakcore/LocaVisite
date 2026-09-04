import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { obtenirLogement } from '../api/logements'
import { demanderVisite } from '../api/visites'
import { messageErreur } from '../api/client'
import { aujourdhui, formaterDate } from '../format'
import Chargement from '../components/Chargement'
import MessageErreur from '../components/MessageErreur'
import Introuvable from '../components/Introuvable'

const SAISIE_VIDE = {
  nom: '',
  prenom: '',
  courriel: '',
  telephone: '',
  possedeMobile: false,
  dateSouhaitee: '',
  heureSouhaiteeDebut: '',
  heureSouhaiteeFin: '',
}

/**
 * Validation avant envoi.
 *
 * L'API revalide tout de son cote : ces controles evitent seulement un
 * aller-retour inutile, ils ne remplacent pas ceux du serveur.
 */
function valider(saisie) {
  if (!saisie.nom.trim()) return 'Le nom est obligatoire.'
  // Le prenom est obligatoire cote API (DemandeVisiteDto.Prenom est [Required]),
  // meme si l'enonce de la partie 4 ne le listait pas. On l'exige ici pour
  // eviter un 400 dont le message serait en anglais.
  if (!saisie.prenom.trim()) return 'Le prénom est obligatoire.'
  if (!saisie.courriel.trim()) return 'Le courriel est obligatoire.'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(saisie.courriel)) {
    return "Le format du courriel n'est pas valide."
  }
  if (!saisie.dateSouhaitee) return 'La date souhaitée est obligatoire.'
  if (!saisie.heureSouhaiteeDebut || !saisie.heureSouhaiteeFin) {
    return 'Les heures de début et de fin sont obligatoires.'
  }
  if (saisie.heureSouhaiteeFin <= saisie.heureSouhaiteeDebut) {
    return "L'heure de fin doit être postérieure à l'heure de début."
  }
  if (saisie.dateSouhaitee < aujourdhui()) {
    return 'La date souhaitée ne peut pas être dans le passé.'
  }

  return null
}

export default function DemandeVisitePage() {
  const { id } = useParams()
  const [logement, setLogement] = useState(null)
  const [chargement, setChargement] = useState(true)
  const [introuvable, setIntrouvable] = useState(false)

  const [saisie, setSaisie] = useState(SAISIE_VIDE)
  const [erreur, setErreur] = useState(null)
  const [envoi, setEnvoi] = useState(false)
  const [confirmation, setConfirmation] = useState(null)

  useEffect(() => {
    let actif = true

    async function charger() {
      try {
        const donnees = await obtenirLogement(id)
        if (actif) setLogement(donnees)
      } catch (probleme) {
        if (!actif) return
        if (probleme.response?.status === 404) setIntrouvable(true)
        else setErreur(messageErreur(probleme))
      } finally {
        if (actif) setChargement(false)
      }
    }

    charger()
    return () => { actif = false }
  }, [id])

  function modifier(champ, valeur) {
    setSaisie((precedents) => ({ ...precedents, [champ]: valeur }))
  }

  async function soumettre(evenement) {
    evenement.preventDefault()

    const probleme = valider(saisie)
    if (probleme) {
      setErreur(probleme)
      return
    }

    setEnvoi(true)
    setErreur(null)

    try {
      const reponse = await demanderVisite({
        idLogement: Number(id),
        ...saisie,
        telephone: saisie.telephone.trim() || null,
      })

      setConfirmation(reponse)
    } catch (souci) {
      // Le message vient de l'API : « Le logement 2 n'est pas offert en
      // visite », « La date souhaitee ne peut pas etre dans le passe »…
      setErreur(messageErreur(souci))
    } finally {
      setEnvoi(false)
    }
  }

  if (chargement) return <Chargement />
  if (introuvable) return <Introuvable />

  if (confirmation) {
    return (
      <section className="confirmation">
        <h1>Votre demande est enregistrée</h1>
        <p>{confirmation.message}</p>

        <dl className="fiche__caracteristiques">
          <div><dt>Logement</dt><dd>{logement.adresse}, {logement.ville}</dd></div>
          <div><dt>Date souhaitée</dt><dd>{formaterDate(saisie.dateSouhaitee)}</dd></div>
          <div>
            <dt>Plage souhaitée</dt>
            <dd>de {saisie.heureSouhaiteeDebut} à {saisie.heureSouhaiteeFin}</dd>
          </div>
          <div><dt>Numéro de demande</dt><dd>{confirmation.idVisite}</dd></div>
        </dl>

        <Link to="/" className="bouton">Retour au catalogue</Link>
      </section>
    )
  }

  return (
    <section className="formulaire">
      <h1>Demander une visite</h1>

      {/* L'utilisateur doit toujours voir de quel logement il s'agit. */}
      {logement && (
        <p className="formulaire__logement">
          {logement.adresse}, {logement.ville} — {logement.type}, {logement.nbPieces} pièces
        </p>
      )}

      <form onSubmit={soumettre} noValidate>
        <div className="formulaire__ligne">
          <label className="champ">
            <span>Nom *</span>
            <input type="text" value={saisie.nom}
              onChange={(e) => modifier('nom', e.target.value)} required />
          </label>

          <label className="champ">
            <span>Prénom *</span>
            <input type="text" value={saisie.prenom}
              onChange={(e) => modifier('prenom', e.target.value)} required />
          </label>
        </div>

        <div className="formulaire__ligne">
          <label className="champ">
            <span>Courriel *</span>
            <input type="email" value={saisie.courriel}
              onChange={(e) => modifier('courriel', e.target.value)} required />
          </label>

          <label className="champ">
            <span>Téléphone</span>
            <input type="tel" value={saisie.telephone}
              onChange={(e) => modifier('telephone', e.target.value)}
              placeholder="450-555-0142" />
          </label>
        </div>

        <label className="champ champ--case">
          <input type="checkbox" checked={saisie.possedeMobile}
            onChange={(e) => modifier('possedeMobile', e.target.checked)} />
          <span>Je possède un téléphone mobile</span>
        </label>

        <label className="champ">
          <span>Date souhaitée *</span>
          <input type="date" value={saisie.dateSouhaitee} min={aujourdhui()}
            onChange={(e) => modifier('dateSouhaitee', e.target.value)} required />
        </label>

        <div className="formulaire__ligne">
          <label className="champ">
            <span>À partir de *</span>
            <input type="time" value={saisie.heureSouhaiteeDebut}
              onChange={(e) => modifier('heureSouhaiteeDebut', e.target.value)} required />
          </label>

          <label className="champ">
            <span>Jusqu'à *</span>
            <input type="time" value={saisie.heureSouhaiteeFin}
              onChange={(e) => modifier('heureSouhaiteeFin', e.target.value)} required />
          </label>
        </div>

        <p className="formulaire__note">
          Proposez une plage assez large : un préposé y placera la visite selon
          les disponibilités de nos agents.
        </p>

        {erreur && <MessageErreur message={erreur} />}

        <div className="fiche__actions">
          <button type="submit" className="bouton" disabled={envoi}>
            {envoi ? 'Envoi…' : 'Envoyer la demande'}
          </button>
          <Link to={`/logements/${id}`} className="bouton bouton--secondaire">Annuler</Link>
        </div>
      </form>
    </section>
  )
}
