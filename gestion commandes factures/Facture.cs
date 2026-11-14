public class Facture
{
    private readonly List<Paiement> _paiements = new();
    private readonly List<Relance> _relances = new();

    public int IdFacture { get; }
    public BonCommande Commande { get; }
    public DateTime Date { get; }
    public DateTime DateEcheance { get; }
    public decimal MontantHT { get; }
    public decimal TauxTVA { get; }
    public StatutFacture Statut { get; private set; }
    public IReadOnlyList<Paiement> Paiements => _paiements;
    public IReadOnlyList<Relance> Relances => _relances;

    public Facture(int idFacture, BonCommande commande, decimal montantHT,
                   DateTime dateEcheance, decimal tauxTva = 0.19m)
    {
        IdFacture = idFacture;
        Commande = commande;
        Date = DateTime.Today;
        DateEcheance = dateEcheance;
        MontantHT = montantHT;
        TauxTVA = tauxTva;
        Statut = StatutFacture.Emise;
    }

    public decimal CalculerTTC() => MontantHT * (1 + TauxTVA);

    public decimal MontantPaye() => _paiements.Sum(p => p.Montant);

    public decimal ResteAPayer() => CalculerTTC() - MontantPaye();

    public void EnregistrerPaiement(decimal montant, ModePaiement mode)
    {
        if (montant <= 0 || montant > ResteAPayer())
            throw new ArgumentException("Montant de paiement invalide.");

        _paiements.Add(new Paiement(_paiements.Count + 1, montant, mode));

        Statut = ResteAPayer() == 0
            ? StatutFacture.Payee
            : StatutFacture.PartiellementPayee;
    }

    public bool EstEnRetard() => Statut != StatutFacture.Payee && DateTime.Today > DateEcheance;

    public Relance Relancer()
    {
        if (!EstEnRetard())
            throw new InvalidOperationException("La facture n'est pas en retard.");

        Statut = StatutFacture.EnRetard;
        int niveau = _relances.Count + 1;
        var relance = new Relance(niveau, niveau,
            $"Facture {IdFacture} impayée : reste {ResteAPayer():F2} DT");
        _relances.Add(relance);
        relance.Envoyer();
        return relance;
    }
}

