public class LigneCommande
{
    public Produit Produit { get; }
    public int Quantite { get; }
    public decimal PrixUnitaire { get; }

    public LigneCommande(Produit produit, int quantite, decimal prixUnitaire)
    {
        Produit = produit;
        Quantite = quantite;
        PrixUnitaire = prixUnitaire;
    }

    public decimal CalculerSousTotal() => Quantite * PrixUnitaire;
}

public class BonCommande
{
    private readonly List<LigneCommande> _lignes = new();

    public int IdCommande { get; }
    public Client Client { get; }
    public DateTime Date { get; }
    public StatutCommande Statut { get; private set; }
    public IReadOnlyList<LigneCommande> Lignes => _lignes;

    public BonCommande(int idCommande, Client client)
    {
        IdCommande = idCommande;
        Client = client;
        Date = DateTime.Today;
        Statut = StatutCommande.EnCours;
    }

    public void AjouterLigne(Produit produit, int quantite, decimal prixUnitaire)
    {
        _lignes.Add(new LigneCommande(produit, quantite, prixUnitaire));
    }

    public decimal CalculerTotal() => _lignes.Sum(l => l.CalculerSousTotal());

    public void Valider()
    {
        if (Statut != StatutCommande.EnCours)
            throw new InvalidOperationException("Seule une commande en cours peut être validée.");

        // On vérifie tout le stock avant de modifier quoi que ce soit
        foreach (var l in _lignes)
        {
            if (!l.Produit.VerifierStock(l.Quantite))
                throw new InvalidOperationException($"Stock insuffisant pour {l.Produit.Designation}.");
        }
        foreach (var l in _lignes)
            l.Produit.DiminuerStock(l.Quantite);

        Statut = StatutCommande.Validee;
    }

    public void Annuler() => Statut = StatutCommande.Annulee;

    public Facture GenererFacture(int idFacture, DateTime dateEcheance, decimal tauxTva = 0.19m)
    {
        if (Statut != StatutCommande.Validee)
            throw new InvalidOperationException("La commande doit être validée.");
        return new Facture(idFacture, this, CalculerTotal(), dateEcheance, tauxTva);
    }
}
