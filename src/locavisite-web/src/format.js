// Mise en forme quebecoise : dollars canadiens et dates en francais.

const argent = new Intl.NumberFormat('fr-CA', {
  style: 'currency',
  currency: 'CAD',
  maximumFractionDigits: 0,
})

export function formaterLoyer(montant) {
  return `${argent.format(montant)} / mois`
}

/** « 2026-09-15 » devient « 15 septembre 2026 ». */
export function formaterDate(dateIso) {
  if (!dateIso) return ''

  const [annee, mois, jour] = dateIso.split('-').map(Number)

  return new Date(annee, mois - 1, jour).toLocaleDateString('fr-CA', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
}

/** Date du jour au format « aaaa-mm-jj », pour l'attribut min des champs date. */
export function aujourdhui() {
  const maintenant = new Date()
  const mois = String(maintenant.getMonth() + 1).padStart(2, '0')
  const jour = String(maintenant.getDate()).padStart(2, '0')

  return `${maintenant.getFullYear()}-${mois}-${jour}`
}

/** Jours de la semaine, indexes comme DayOfWeek : 0 = dimanche. */
export const JOURS_SEMAINE = [
  'dimanche', 'lundi', 'mardi', 'mercredi', 'jeudi', 'vendredi', 'samedi',
]
