import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listerVisites } from '../../api/visites'
import { messageErreur } from '../../api/client'
import { formaterDate } from '../../format'
import Chargement from '../../components/Chargement'
import MessageErreur from '../../components/MessageErreur'
import EtiquetteStatut from '../../components/EtiquetteStatut'

const STATUTS = ['DEMANDEE', 'ASSIGNEE', 'EN_ROUTE', 'EN_COURS', 'TERMINEE', 'ANNULEE']

export default function VisitesGestionPage() {
  const [visites, setVisites] = useState([])
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState(null)
  const [filtres, setFiltres] = useState({ statut: '', date: '' })
  const [filtresActifs, setFiltresActifs] = useState({ statut: '', date: '' })

  const charger = useCallback(async (criteres) => {
    setChargement(true)
    setErreur(null)

    try {
      setVisites(await listerVisites(criteres))
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    } finally {
      setChargement(false)
    }
  }, [])

  useEffect(() => { charger(filtresActifs) }, [charger, filtresActifs])

  function soumettre(evenement) {
    evenement.preventDefault()
    setFiltresActifs(filtres)
  }

  function reinitialiser() {
    const vide = { statut: '', date: '' }
    setFiltres(vide)
    setFiltresActifs(vide)
  }

  return (
    <section>
      <div className="entete-page">
        <h1>Visites</h1>
      </div>

      {/* Les filtres partent a l'API, comme pour le catalogue public. */}
      <form className="filtres" onSubmit={soumettre}>
        <div className="filtres__champs filtres__champs--deux">
          <label className="champ">
            <span>Statut</span>
            <select value={filtres.statut}
              onChange={(e) => setFiltres({ ...filtres, statut: e.target.value })}>
              <option value="">Tous les statuts</option>
              {STATUTS.map((s) => <option key={s} value={s}>{s.replace('_', ' ')}</option>)}
            </select>
          </label>

          <label className="champ">
            <span>Date</span>
            <input type="date" value={filtres.date}
              onChange={(e) => setFiltres({ ...filtres, date: e.target.value })} />
          </label>
        </div>

        <div className="filtres__actions">
          <button type="submit" className="bouton">Filtrer</button>
          <button type="button" className="bouton bouton--secondaire" onClick={reinitialiser}>
            Réinitialiser
          </button>
        </div>
      </form>

      {chargement && <Chargement texte="Chargement des visites…" />}
      {!chargement && erreur && (
        <MessageErreur message={erreur} onReessayer={() => charger(filtresActifs)} />
      )}

      {!chargement && !erreur && visites.length === 0 && (
        <p className="etat">Aucune visite ne correspond à ces critères.</p>
      )}

      {!chargement && !erreur && visites.length > 0 && (
        <div className="tableau-defilant">
          <table className="tableau">
            <thead>
              <tr>
                <th>Nº</th>
                <th>Logement</th>
                <th>Prospect</th>
                <th>Plage souhaitée</th>
                <th>Horaire retenu</th>
                <th>Statut</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {visites.map((visite) => (
                <tr key={visite.idVisite}>
                  <td>{visite.idVisite}</td>
                  <td>
                    {visite.logement
                      ? `${visite.logement.adresse}, ${visite.logement.ville}`
                      : '—'}
                  </td>
                  <td>
                    {visite.prospect
                      ? `${visite.prospect.prenom} ${visite.prospect.nom}`.trim()
                      : '—'}
                  </td>
                  <td>
                    {formaterDate(visite.dateSouhaitee)}
                    <br />
                    <span className="discret">
                      {visite.heureSouhaiteeDebut} – {visite.heureSouhaiteeFin}
                    </span>
                  </td>
                  <td>
                    {visite.datePrevue
                      ? <>{formaterDate(visite.datePrevue)}<br />
                          <span className="discret">
                            {visite.heurePrevue} · {visite.dureePrevue} min
                          </span>
                        </>
                      : <span className="discret">non assignée</span>}
                  </td>
                  <td><EtiquetteStatut statut={visite.statut} /></td>
                  <td>
                    <Link to={`/gestion/visites/${visite.idVisite}`}
                      className="bouton bouton--petit bouton--secondaire">
                      Ouvrir
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
