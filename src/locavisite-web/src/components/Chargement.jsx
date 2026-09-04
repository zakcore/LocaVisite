export default function Chargement({ texte = 'Chargement…' }) {
  return (
    <p className="etat etat--chargement" role="status">
      {texte}
    </p>
  )
}
