public class Relance
{
    public int IdRelance { get; }
    public DateTime Date { get; }
    public int Niveau { get; }
    public string Message { get; }

    public Relance(int idRelance, int niveau, string message)
    {
        IdRelance = idRelance;
        Date = DateTime.Today;
        Niveau = niveau;
        Message = message;
    }

    public void Envoyer()
    {
        Console.WriteLine($"[Relance n°{Niveau}] {Message}");
    }
}
