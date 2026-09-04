import { useState } from 'react'

const FILTRES_VIDES = { ville: '', loyerMax: '', nbPiecesMin: '', type: '' }

/**
 * Barre de filtres du catalogue.
 *
 * Les filtres ne sont appliques qu'a la soumission : chaque changement
 * n'declenche pas un appel a l'API.
 */
export default function FiltresLogements({ onFiltrer }) {
  const [filtres, setFiltres] = useState(FILTRES_VIDES)

  function modifier(champ, valeur) {
    setFiltres((precedents) => ({ ...precedents, [champ]: valeur }))
  }

  function soumettre(evenement) {
    evenement.preventDefault()
    onFiltrer(filtres)
  }

  function reinitialiser() {
    setFiltres(FILTRES_VIDES)
    onFiltrer(FILTRES_VIDES)
  }

  return (
    <form className="filtres" onSubmit={soumettre}>
      <div className="filtres__champs">
        <label className="champ">
          <span>Ville</span>
          <input
            type="text"
            value={filtres.ville}
            onChange={(e) => modifier('ville', e.target.value)}
            placeholder="Saint-Jean-sur-Richelieu"
          />
        </label>

        <label className="champ">
          <span>Loyer maximum</span>
          <input
            type="number"
            min="0"
            step="50"
            value={filtres.loyerMax}
            onChange={(e) => modifier('loyerMax', e.target.value)}
            placeholder="1200"
          />
        </label>

        <label className="champ">
          <span>Pièces minimum</span>
          <input
            type="number"
            min="1"
            max="20"
            value={filtres.nbPiecesMin}
            onChange={(e) => modifier('nbPiecesMin', e.target.value)}
            placeholder="3"
          />
        </label>

        <label className="champ">
          <span>Type</span>
          <input
            type="text"
            value={filtres.type}
            onChange={(e) => modifier('type', e.target.value)}
            placeholder="Appartement"
          />
        </label>
      </div>

      <div className="filtres__actions">
        <button type="submit" className="bouton">Filtrer</button>
        <button type="button" className="bouton bouton--secondaire" onClick={reinitialiser}>
          Réinitialiser
        </button>
      </div>
    </form>
  )
}
