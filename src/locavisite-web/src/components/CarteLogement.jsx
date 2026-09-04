import { Link } from 'react-router-dom'
import { formaterLoyer } from '../format'

export default function CarteLogement({ logement }) {
  return (
    <li className="carte">
      <Link to={`/logements/${logement.idLogement}`} className="carte__lien">
        <p className="carte__ville">{logement.ville}</p>
        <h2 className="carte__adresse">{logement.adresse}</h2>

        <p className="carte__details">
          {logement.type} · {logement.nbPieces} pièces
        </p>

        <p className="carte__loyer">{formaterLoyer(logement.loyerMensuel)}</p>
      </Link>
    </li>
  )
}
