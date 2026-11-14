public class Produit
{
    public int IdProduit { get; }
    public string Designation { get; private set; }
    public decimal PrixUnitaire { get; private set; }
    public int Stock { get; private set; }

    public Produit(int idProduit, string designation, decimal prixUnitaire, int stock)
    {
        IdProduit = idProduit;
        Designation = designation;
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
