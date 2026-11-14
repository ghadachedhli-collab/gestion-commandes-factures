public class Paiement
{
    public int IdPaiement { get; }
    public DateTime Date { get; }
    public decimal Montant { get; }
    public ModePaiement Mode { get; }

    public Paiement(int idPaiement, decimal montant, ModePaiement mode)
    {
        IdPaiement = idPaiement;
        Date = DateTime.Today;
        Montant = montant;
        Mode = mode;
    }
}

