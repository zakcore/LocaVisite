import { useCallback, useEffect, useState } from 'react'
import { listerLogements } from '../api/logements'
import { messageErreur } from '../api/client'
import CarteLogement from '../components/CarteLogement'
import FiltresLogements from '../components/FiltresLogements'
import Chargement from '../components/Chargement'
import MessageErreur from '../components/MessageErreur'

export default function CataloguePage() {
  const [logements, setLogements] = useState([])
  const [filtres, setFiltres] = useState({})
  const [chargement, setChargement] = useState(true)
  const [erreur, setErreur] = useState(null)

  const charger = useCallback(async (filtresActifs) => {
    setChargement(true)
    setErreur(null)

    try {
      setLogements(await listerLogements(filtresActifs))
    } catch (probleme) {
      setErreur(messageErreur(probleme))
    } finally {
      setChargement(false)
    }
  }, [])

  useEffect(() => {
    charger(filtres)
  }, [charger, filtres])

  return (
    <section>
      <h1>Logements à louer</h1>

      <FiltresLogements onFiltrer={setFiltres} />

      {chargement && <Chargement texte="Chargement des logements…" />}

      {/* Une erreur reseau et un filtre sans resultat sont deux situations
          differentes : l'une se reessaie, l'autre demande d'elargir sa
          recherche. */}
      {!chargement && erreur && (
        <MessageErreur message={erreur} onReessayer={() => charger(filtres)} />
      )}

      {!chargement && !erreur && logements.length === 0 && (
        <p className="etat">
          Aucun logement ne correspond à votre recherche. Essayez d'élargir vos critères.
        </p>
      )}

      {!chargement && !erreur && logements.length > 0 && (
        <>
          <p className="compte">
            {logements.length} logement{logements.length > 1 ? 's' : ''} disponible
            {logements.length > 1 ? 's' : ''}
          </p>

          <ul className="grille">
            {logements.map((logement) => (
              <CarteLogement key={logement.idLogement} logement={logement} />
            ))}
          </ul>
        </>
      )}
    </section>
  )
}
