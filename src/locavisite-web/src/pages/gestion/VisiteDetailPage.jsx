import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  annulerVisite,
  assignerVisite,
  chercherAgentsDisponibles,
  fixerDuree,
  obtenirVisite,
} from '../../api/visites'
import { messageErreur } from '../../api/client'
import { formaterDate, formaterLoyer } from '../../format'
import Chargement from '../../components/Chargement'
import MessageErreur from '../../components/MessageErreur'
import EtiquetteStatut from '../../components/EtiquetteStatut'
import Introuvable from '../../components/Introuvable'

/** Durées acceptées par l'API : de 15 à 240 minutes, par pas de 15. */
const DUREES = Array.from({ length: 16 }, (_, i) => (i + 1) * 15)

export default function VisiteDetailPage() {
  const { id } = useParams()

  const [visite, setVisite] = useState(null)
  const [chargement, setChargement] = useState(true)
  const [introuvable, setIntrouvable] = useState(false)
  const [erreur, setErreur] = useState(null)

  const [duree, setDuree] = useState(30)
  const [messageDuree, setMessageDuree] = useState(null)
  const [erreurAction, setErreurAction] = useState(null)

  const [recherche, setRecherche] = useState(null)
  const [rechercheEnCours, setRechercheEnCours] = useState(false)
  const [assignationEnCours, setAssignationEnCours] = useState(null)

  const charger = useCallback(async () => {
    setChargement(true)
    setErreur(null)

    try {
      const donnees = await obtenirVisite(id)
      setVisite(donnees)
      setDuree(donnees.dureePrevue)
    } catch (probleme) {
      if (probleme.response?.status === 404) setIntrouvable(true)
      else setErreur(messageErreur(probleme))
    } finally {
      setChargement(false)
    }
  }, [id])

  useEffect(() => { charger() }, [charger])

  // conserverErreur : la relance automatique apres un 409 ne doit pas effacer
  // le message qui explique justement pourquoi l'assignation a echoue.
  const chercherCreneaux = useCallback(async (conserverErreur = false) => {
    setRechercheEnCours(true)
    if (!conserverErreur) setErreurAction(null)

    try {
      setRecherche(await chercherAgentsDisponibles(id))
    } catch (probleme) {
      setErreurAction(messageErreur(probleme))
      setRecherche(null)
    } finally {
      setRechercheEnCours(false)
    }
  }, [id])

  async function enregistrerDuree() {
    setErreurAction(null)
    setMessageDuree(null)

    try {
      const misAJour = await fixerDuree(id, Number(duree))
      setVisite(misAJour)
      setMessageDuree(`Durée fixée à ${misAJour.dureePrevue} minutes.`)

      // La durée change les créneaux possibles : l'ancienne liste n'a plus
      // de sens, on la relance si elle était déjà affichée.
      if (recherche) await chercherCreneaux()
    } catch (probleme) {
      setErreurAction(messageErreur(probleme))
    }
  }

  async function assigner(creneau) {
    setAssignationEnCours(`${creneau.idAgent}-${creneau.heureDebut}`)
    setErreurAction(null)

    try {
      const misAJour = await assignerVisite(id, creneau.idAgent, creneau.heureDebut)
      setVisite(misAJour)
      setRecherche(null)
    } catch (probleme) {
      setErreurAction(messageErreur(probleme))

      // 409 : le créneau vient d'être pris par un autre préposé. On relance
      // la recherche pour montrer immédiatement ce qui reste libre.
      if (probleme.response?.status === 409) {
        await chercherCreneaux(true)
      }
    } finally {
      setAssignationEnCours(null)
    }
  }

  async function annuler() {
    const confirme = window.confirm(
      `Annuler la visite nº ${id} ?\n\nCette action est définitive.`,
    )
    if (!confirme) return

    setErreurAction(null)

    try {
      setVisite(await annulerVisite(id))
      setRecherche(null)
    } catch (probleme) {
      setErreurAction(messageErreur(probleme))
    }
  }

  if (chargement) return <Chargement />
  if (introuvable) return <Introuvable />
  if (erreur) return <MessageErreur message={erreur} onReessayer={charger} />
  if (!visite) return null

  const estDemandee = visite.statut === 'DEMANDEE'
  const annulable = estDemandee || visite.statut === 'ASSIGNEE'

  return (
    <section>
      <div className="entete-page">
        <h1>Visite nº {visite.idVisite}</h1>
        <EtiquetteStatut statut={visite.statut} />
      </div>

      <div className="panneau">
        <h2>Demande</h2>
        <dl className="fiche__caracteristiques">
          <div>
            <dt>Logement</dt>
            <dd>{visite.logement ? `${visite.logement.adresse}, ${visite.logement.ville}` : '—'}</dd>
          </div>
          <div>
            <dt>Loyer</dt>
            <dd>{visite.logement ? formaterLoyer(visite.logement.loyerMensuel) : '—'}</dd>
          </div>
          <div>
            <dt>Prospect</dt>
            <dd>{visite.prospect ? `${visite.prospect.prenom} ${visite.prospect.nom}`.trim() : '—'}</dd>
          </div>
          <div>
            <dt>Coordonnées</dt>
            <dd>
              {visite.prospect?.courriel}
              {visite.prospect?.telephone ? <><br />{visite.prospect.telephone}</> : null}
            </dd>
          </div>
          <div>
            <dt>Date souhaitée</dt>
            <dd>{formaterDate(visite.dateSouhaitee)}</dd>
          </div>
          <div>
            <dt>Plage souhaitée</dt>
            <dd>{visite.heureSouhaiteeDebut} – {visite.heureSouhaiteeFin}</dd>
          </div>
        </dl>

        {visite.datePrevue && (
          <>
            <h2>Horaire retenu</h2>
            <dl className="fiche__caracteristiques">
              <div><dt>Date</dt><dd>{formaterDate(visite.datePrevue)}</dd></div>
              <div><dt>Heure</dt><dd>{visite.heurePrevue}</dd></div>
              <div><dt>Durée</dt><dd>{visite.dureePrevue} minutes</dd></div>
              <div><dt>Agent</dt><dd>nº {visite.idAgent}</dd></div>
            </dl>
          </>
        )}
      </div>

      {erreurAction && (
        <div style={{ marginBottom: '1rem' }}><MessageErreur message={erreurAction} /></div>
      )}

      {estDemandee && (
        <div className="panneau">
          <h2>Assigner un agent</h2>

          <div className="assignation__duree">
            <label className="champ">
              <span>Durée prévue</span>
              <select value={duree} onChange={(e) => setDuree(e.target.value)}>
                {DUREES.map((d) => <option key={d} value={d}>{d} minutes</option>)}
              </select>
            </label>

            <button type="button" className="bouton bouton--secondaire" onClick={enregistrerDuree}>
              Enregistrer la durée
            </button>

            <button type="button" className="bouton" onClick={() => chercherCreneaux()}
              disabled={rechercheEnCours}>
              {rechercheEnCours ? 'Recherche…' : 'Chercher les agents disponibles'}
            </button>
          </div>

          {messageDuree && <p className="message-succes">{messageDuree}</p>}

          {recherche && (
            <div className="creneaux">
              <p className="discret">
                Durée requise : {recherche.dureeRequise} minutes · plage demandée{' '}
                {recherche.plageDemandee.debut} – {recherche.plageDemandee.fin} le{' '}
                {formaterDate(recherche.plageDemandee.date)}
              </p>

              {/* Une liste vide sans explication laisserait croire que tous les
                  agents sont occupes : c'est exactement ce que le champ
                  « raison » sert a eviter. */}
              {recherche.creneaux.length === 0 ? (
                <div className="etat etat--avertissement" role="status">
                  <strong>Aucun créneau disponible.</strong>
                  <p>{recherche.raison}</p>
                </div>
              ) : (
                <ul className="creneaux__liste">
                  {recherche.creneaux.map((creneau) => {
                    const cle = `${creneau.idAgent}-${creneau.heureDebut}`

                    return (
                      <li key={cle} className="creneau">
                        <div>
                          <strong>{creneau.nomAgent}</strong>
                          <span className="discret">
                            {' '}de {creneau.heureDebut} à {creneau.heureFin}
                          </span>
                        </div>
                        <button type="button" className="bouton bouton--petit"
                          onClick={() => assigner(creneau)}
                          disabled={assignationEnCours !== null}>
                          {assignationEnCours === cle ? 'Assignation…' : 'Assigner'}
                        </button>
                      </li>
                    )
                  })}
                </ul>
              )}
            </div>
          )}
        </div>
      )}

      <div className="fiche__actions">
        <Link to="/gestion/visites" className="bouton bouton--secondaire">
          Retour aux visites
        </Link>
        {annulable && (
          <button type="button" className="bouton bouton--danger" onClick={annuler}>
            Annuler la visite
          </button>
        )}
      </div>
    </section>
  )
}
