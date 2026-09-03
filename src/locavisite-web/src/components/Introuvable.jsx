import { Link } from 'react-router-dom'

export default function Introuvable() {
  return (
    <section className="etat">
      <h1>Page introuvable</h1>
      <p>Le logement que vous cherchez n'existe pas ou n'est plus au catalogue.</p>
      <Link to="/" className="bouton">Retour au catalogue</Link>
    </section>
  )
}
