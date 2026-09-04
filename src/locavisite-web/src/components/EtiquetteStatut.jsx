/**
 * Statut d'une visite ou d'un logement.
 *
 * La couleur seule ne suffit pas : chaque statut garde son libelle en toutes
 * lettres, lisible sans distinguer les couleurs.
 */
export default function EtiquetteStatut({ statut }) {
  return (
    <span className={`etiquette etiquette--${statut.toLowerCase()}`}>
      {statut.replace('_', ' ')}
    </span>
  )
}
