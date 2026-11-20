public class Produit
{
    public int IdProduit { get; private set; }
    public string Designation { get; private set; }
    public decimal PrixUnitaire { get; private set; }
    public int Stock { get; private set; }

    private Produit()
    {
        Designation = string.Empty;
    }

    public Produit(int idProduit, string designation, decimal prixUnitaire, int stock)
    {
        IdProduit = idProduit;
        Designation = designation;
        PrixUnitaire = prixUnitaire;
        Stock = stock;
    }

    public void ModifierInfos(string designation, decimal prixUnitaire, int stock)
    {
        if (string.IsNullOrWhiteSpace(designation))
            throw new ArgumentException("La désignation est obligatoire.");
        if (prixUnitaire < 0)
            throw new ArgumentException("Le prix doit être positif.");
        if (stock < 0)
            throw new ArgumentException("Le stock doit être positif.");

        Designation = designation.Trim();
        PrixUnitaire = prixUnitaire;
        Stock = stock;
    }

    public void ModifierPrix(decimal nouveauPrix)
    {
        if (nouveauPrix < 0)
            throw new ArgumentException("Le prix doit être positif.");
        PrixUnitaire = nouveauPrix;
    }

    public bool VerifierStock(int quantite) => Stock >= quantite;

    public void DiminuerStock(int quantite)
    {
        if (!VerifierStock(quantite))
            throw new InvalidOperationException($"Stock insuffisant pour {Designation}.");
        Stock -= quantite;
    }

    public override string ToString() => $"{Designation} ({PrixUnitaire:F2} DT)";
}
