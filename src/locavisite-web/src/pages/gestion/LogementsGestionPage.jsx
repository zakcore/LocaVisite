import { useCallback, useEffect, useState } from 'react'
import {
  creerLogement,
  listerTousLesLogements,
  modifierLogement,
  retirerLogement,
} from '../../api/logements'
import { messageErreur } from '../../api/client'
import { formaterLoyer } from '../../format'
import Chargement from '../../components/Chargement'
import MessageErreur from '../../components/MessageErreur'
import EtiquetteStatut from '../../components/EtiquetteStatut'

const STATUTS = ['DISPONIBLE', 'RESERVE', 'LOUE', 'RETIRE']

const SAISIE_VIDE = {
  adresse: '',
  ville: '',
  codePostal: '',
  type: '',
  nbPieces: 1,
  loyerMensuel: 0,
  description: '',
  statut: 'DISPONIBLE',
}

export default function LogementsGestionPage() {
  const [logements, setLogements] = useState([])
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState(null)

  const [saisie, setSaisie] = useState(SAISIE_VIDE)
  const [idModifie, setIdModifie] = useState(null)
  const [formulaireOuvert, setFormulaireOuvert] = useState(false)
  const [erreurFormulaire, setErreurFormulaire] = useState(null)
  const [envoi, setEnvoi] = useState(false)

  const charger = useCallback(async () => {
    setChargement(true)
    setErreur(null)

    try {
      setLogements(await listerTousLesLogements())
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    } finally {
      setChargement(false)
    }
  }, [])

  useEffect(() => { charger() }, [charger])

  function ouvrirCreation() {
    setSaisie(SAISIE_VIDE)
    setIdModifie(null)
    setErreurFormulaire(null)
    setFormulaireOuvert(true)
  }

  function ouvrirModification(logement) {
    setSaisie({
      adresse: logement.adresse,
      ville: logement.ville,
      codePostal: logement.codePostal,
      type: logement.type,
      nbPieces: logement.nbPieces,
      loyerMensuel: logement.loyerMensuel,
      description: logement.description ?? '',
      statut: logement.statut,
    })
    setIdModifie(logement.idLogement)
    setErreurFormulaire(null)
    setFormulaireOuvert(true)
  }

  function modifier(champ, valeur) {
    setSaisie((precedents) => ({ ...precedents, [champ]: valeur }))
  }

  async function soumettre(evenement) {
    evenement.preventDefault()
    setErreurFormulaire(null)
    setEnvoi(true)

    const corps = {
      ...saisie,
      nbPieces: Number(saisie.nbPieces),
      loyerMensuel: Number(saisie.loyerMensuel),
      description: saisie.description.trim() || null,
    }

    try {
      if (idModifie) {
        await modifierLogement(idModifie, corps)
      } else {
        await creerLogement(corps)
      }

      setFormulaireOuvert(false)
      await charger()
    } catch (probleme) {
      setErreurFormulaire(messageErreur(probleme))
    } finally {
      setEnvoi(false)
    }
  }

  async function retirer(logement) {
    // Le retrait est logique mais reste une action de gestion : on confirme
    // avant d'appeler l'API.
    const confirme = window.confirm(
      `Retirer le logement « ${logement.adresse} » du catalogue ?\n\n`
      + "Il passera au statut RETIRE et ne sera plus visible publiquement.",
    )

    if (!confirme) return

    try {
      await retirerLogement(logement.idLogement)
      await charger()
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    }
  }

  return (
    <section>
      <div className="entete-page">
        <h1>Logements</h1>
        <button type="button" className="bouton" onClick={ouvrirCreation}>
          Nouveau logement
        </button>
      </div>

      {formulaireOuvert && (
        <form className="panneau" onSubmit={soumettre} noValidate>
          <h2>{idModifie ? 'Modifier le logement' : 'Nouveau logement'}</h2>

          <div className="formulaire__ligne">
            <label className="champ">
              <span>Adresse *</span>
              <input type="text" value={saisie.adresse} required
                onChange={(e) => modifier('adresse', e.target.value)} />
            </label>
            <label className="champ">
              <span>Ville *</span>
              <input type="text" value={saisie.ville} required
                onChange={(e) => modifier('ville', e.target.value)} />
            </label>
          </div>

          <div className="formulaire__ligne">
            <label className="champ">
              <span>Code postal *</span>
              <input type="text" value={saisie.codePostal} required
                onChange={(e) => modifier('codePostal', e.target.value)} />
            </label>
            <label className="champ">
              <span>Type *</span>
              <input type="text" value={saisie.type} required placeholder="Appartement"
                onChange={(e) => modifier('type', e.target.value)} />
            </label>
          </div>

          <div className="formulaire__ligne">
            <label className="champ">
              <span>Nombre de pièces *</span>
              <input type="number" min="1" max="20" value={saisie.nbPieces} required
                onChange={(e) => modifier('nbPieces', e.target.value)} />
            </label>
            <label className="champ">
              <span>Loyer mensuel *</span>
              <input type="number" min="0" step="25" value={saisie.loyerMensuel} required
                onChange={(e) => modifier('loyerMensuel', e.target.value)} />
            </label>
          </div>

          <label className="champ">
            <span>Statut</span>
            <select value={saisie.statut} onChange={(e) => modifier('statut', e.target.value)}>
              {STATUTS.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </label>

          <label className="champ" style={{ marginTop: '0.9rem' }}>
            <span>Description</span>
            <textarea rows="3" value={saisie.description}
              onChange={(e) => modifier('description', e.target.value)} />
          </label>

          {erreurFormulaire && (
            <div style={{ marginTop: '1rem' }}><MessageErreur message={erreurFormulaire} /></div>
          )}

          <div className="fiche__actions">
            <button type="submit" className="bouton" disabled={envoi}>
              {envoi ? 'Enregistrement…' : 'Enregistrer'}
            </button>
            <button type="button" className="bouton bouton--secondaire"
              onClick={() => setFormulaireOuvert(false)}>
              Annuler
            </button>
          </div>
        </form>
      )}

      {chargement && <Chargement />}
      {!chargement && erreur && <MessageErreur message={erreur} onReessayer={charger} />}

      {!chargement && !erreur && (
        <div className="tableau-defilant">
          <table className="tableau">
            <thead>
              <tr>
                <th>Adresse</th>
                <th>Ville</th>
                <th>Type</th>
                <th>Pièces</th>
                <th>Loyer</th>
                <th>Statut</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {logements.map((logement) => (
                <tr key={logement.idLogement}>
                  <td>{logement.adresse}</td>
                  <td>{logement.ville}</td>
                  <td>{logement.type}</td>
                  <td>{logement.nbPieces}</td>
                  <td>{formaterLoyer(logement.loyerMensuel)}</td>
                  <td><EtiquetteStatut statut={logement.statut} /></td>
                  <td className="tableau__actions">
                    <button type="button" className="bouton bouton--petit bouton--secondaire"
                      onClick={() => ouvrirModification(logement)}>
                      Modifier
                    </button>
                    {logement.statut !== 'RETIRE' && (
                      <button type="button" className="bouton bouton--petit bouton--danger"
                        onClick={() => retirer(logement)}>
                        Retirer
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {logements.length === 0 && <p className="etat">Aucun logement enregistré.</p>}
        </div>
      )}
    </section>
  )
}
