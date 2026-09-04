import { useCallback, useEffect, useState } from 'react'
import {
  ajouterDisponibilite,
  listerAgents,
  listerDisponibilites,
  supprimerDisponibilite,
} from '../../api/agents'
import { messageErreur } from '../../api/client'
import { JOURS_SEMAINE } from '../../format'
import Chargement from '../../components/Chargement'
import MessageErreur from '../../components/MessageErreur'

const PLAGE_VIDE = { jourSemaine: 1, heureDebut: '09:00', heureFin: '17:00' }

export default function AgentsGestionPage() {
  const [agents, setAgents] = useState([])
  const [agentChoisi, setAgentChoisi] = useState(null)
  const [disponibilites, setDisponibilites] = useState([])

  const [chargement, setChargement] = useState(true)
  const [chargementPlages, setChargementPlages] = useState(false)
  const [erreur, setErreur] = useState(null)
  const [erreurPlage, setErreurPlage] = useState(null)

  const [plage, setPlage] = useState(PLAGE_VIDE)
  const [envoi, setEnvoi] = useState(false)

  const chargerAgents = useCallback(async () => {
    setChargement(true)
    setErreur(null)

    try {
      const liste = await listerAgents()
      setAgents(liste)
      if (liste.length > 0) setAgentChoisi(liste[0].idUtilisateur)
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    } finally {
      setChargement(false)
    }
  }, [])

  const chargerPlages = useCallback(async (idAgent) => {
    if (!idAgent) return

    setChargementPlages(true)
    setErreurPlage(null)

    try {
      setDisponibilites(await listerDisponibilites(idAgent))
    } catch (probleme) {
      setErreurPlage(messageErreur(probleme))
    } finally {
      setChargementPlages(false)
    }
  }, [])

  useEffect(() => { chargerAgents() }, [chargerAgents])
  useEffect(() => { chargerPlages(agentChoisi) }, [chargerPlages, agentChoisi])

  async function ajouter(evenement) {
    evenement.preventDefault()
    setErreurPlage(null)
    setEnvoi(true)

    try {
      // Le jour part en entier 0 à 6, comme l'attend l'API.
      await ajouterDisponibilite(agentChoisi, {
        jourSemaine: Number(plage.jourSemaine),
        heureDebut: plage.heureDebut,
        heureFin: plage.heureFin,
      })

      await chargerPlages(agentChoisi)
    } catch (probleme) {
      // Notamment le refus d'une plage chevauchante, message venant de l'API.
      setErreurPlage(messageErreur(probleme))
    } finally {
      setEnvoi(false)
    }
  }

  async function supprimer(disponibilite) {
    const confirme = window.confirm(
      `Supprimer la plage du ${JOURS_SEMAINE[disponibilite.jourSemaine]} `
      + `de ${disponibilite.heureDebut} à ${disponibilite.heureFin} ?`,
    )
    if (!confirme) return

    setErreurPlage(null)

    try {
      await supprimerDisponibilite(disponibilite.idDisponibilite)
      await chargerPlages(agentChoisi)
    } catch (probleme) {
      setErreurPlage(messageErreur(probleme))
    }
  }

  if (chargement) return <Chargement />
  if (erreur) return <MessageErreur message={erreur} onReessayer={chargerAgents} />

  return (
    <section>
      <div className="entete-page">
        <h1>Agents et disponibilités</h1>
      </div>

      {agents.length === 0 ? (
        <p className="etat">Aucun agent enregistré.</p>
      ) : (
        <>
          <div className="panneau">
            <label className="champ">
              <span>Agent</span>
              <select value={agentChoisi ?? ''}
                onChange={(e) => setAgentChoisi(Number(e.target.value))}>
                {agents.map((agent) => (
                  <option key={agent.idUtilisateur} value={agent.idUtilisateur}>
                    {agent.prenom} {agent.nom} — {agent.courriel}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="panneau">
            <h2>Plages hebdomadaires</h2>

            {chargementPlages && <Chargement />}

            {!chargementPlages && disponibilites.length === 0 && (
              <p className="discret">Cet agent n'a déclaré aucune disponibilité.</p>
            )}

            {!chargementPlages && disponibilites.length > 0 && (
              <ul className="plages">
                {disponibilites.map((d) => (
                  <li key={d.idDisponibilite} className="plage">
                    <span>
                      <strong className="plage__jour">{d.nomJour}</strong>
                      <span className="discret"> de {d.heureDebut} à {d.heureFin}</span>
                    </span>
                    <button type="button" className="bouton bouton--petit bouton--danger"
                      onClick={() => supprimer(d)}>
                      Supprimer
                    </button>
                  </li>
                ))}
              </ul>
            )}

            <h2>Ajouter une plage</h2>
            <form onSubmit={ajouter}>
              <div className="formulaire__ligne formulaire__ligne--trois">
                <label className="champ">
                  <span>Jour</span>
                  <select value={plage.jourSemaine}
                    onChange={(e) => setPlage({ ...plage, jourSemaine: e.target.value })}>
                    {JOURS_SEMAINE.map((nom, index) => (
                      <option key={nom} value={index}>{nom}</option>
                    ))}
                  </select>
                </label>

                <label className="champ">
                  <span>De</span>
                  <input type="time" value={plage.heureDebut} required
                    onChange={(e) => setPlage({ ...plage, heureDebut: e.target.value })} />
                </label>

                <label className="champ">
                  <span>À</span>
                  <input type="time" value={plage.heureFin} required
                    onChange={(e) => setPlage({ ...plage, heureFin: e.target.value })} />
                </label>
              </div>

              {erreurPlage && (
                <div style={{ margin: '1rem 0' }}><MessageErreur message={erreurPlage} /></div>
              )}

              <button type="submit" className="bouton" disabled={envoi}>
                {envoi ? 'Ajout…' : 'Ajouter la plage'}
              </button>
            </form>
          </div>
        </>
      )}
    </section>
  )
}
