/** Affiche le message venant de l'API, pas un texte generique. */
export default function MessageErreur({ message, onReessayer }) {
  return (
    <div className="etat etat--erreur" role="alert">
      <p>{message}</p>
      {onReessayer && (
        <button type="button" className="bouton bouton--secondaire" onClick={onReessayer}>
          Réessayer
        </button>
      )}
    </div>
  )
}
