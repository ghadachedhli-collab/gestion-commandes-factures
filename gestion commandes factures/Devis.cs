public class LigneDevis
{
    public Produit Produit { get; }
    public int Quantite { get; }
    public decimal PrixUnitaire { get; }   // prix figé à la création

    public LigneDevis(Produit produit, int quantite)
    {
        if (quantite <= 0)
            throw new ArgumentException("La quantité doit être positive.");
        Produit = produit;
        Quantite = quantite;
        PrixUnitaire = produit.PrixUnitaire;
    }

    public decimal CalculerSousTotal() => Quantite * PrixUnitaire;
}

public class Devis
{
    private readonly List<LigneDevis> _lignes = new();

    public int IdDevis { get; }
    public Client Client { get; }
    public DateTime Date { get; }
    public DateTime DateValidite { get; }
    public StatutDevis Statut { get; private set; }
    public IReadOnlyList<LigneDevis> Lignes => _lignes;

    public Devis(int idDevis, Client client, DateTime dateValidite)
    {
        IdDevis = idDevis;
        Client = client;
        Date = DateTime.Today;
        DateValidite = dateValidite;
        Statut = StatutDevis.Brouillon;
    }

    public void AjouterLigne(Produit produit, int quantite)
    {
        _lignes.Add(new LigneDevis(produit, quantite));
    }

    public decimal CalculerTotal() => _lignes.Sum(l => l.CalculerSousTotal());

    public void Accepter()
    {
        if (DateTime.Today > DateValidite)
        {
            Statut = StatutDevis.Expire;
            throw new InvalidOperationException("Le devis est expiré.");
        }
        Statut = StatutDevis.Accepte;
    }

    public void Refuser() => Statut = StatutDevis.Refuse;

    public BonCommande ConvertirEnCommande(int idCommande)
    {
        if (Statut != StatutDevis.Accepte)
            throw new InvalidOperationException("Seul un devis accepté peut devenir une commande.");

        var commande = new BonCommande(idCommande, Client);
        foreach (var l in _lignes)
            commande.AjouterLigne(l.Produit, l.Quantite, l.PrixUnitaire);
        return commande;
    }
}