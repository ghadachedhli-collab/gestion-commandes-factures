public class Client
{
    public int IdClient { get; private set; }
    public string Nom { get; private set; }
    public string Email { get; private set; }
    public string Telephone { get; private set; }
    public string Adresse { get; private set; }

    private Client()
    {
        Nom = string.Empty;
        Email = string.Empty;
        Telephone = string.Empty;
        Adresse = string.Empty;
    }

    public Client(int idClient, string nom, string email, string telephone, string adresse)
    {
        IdClient = idClient;
        Nom = nom;
        Email = email;
        Telephone = telephone;
        Adresse = adresse;
    }

    public void ModifierInfos(string? nom = null, string? email = null,
                              string? telephone = null, string? adresse = null)
    {
        if (!string.IsNullOrWhiteSpace(nom)) Nom = nom;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        if (!string.IsNullOrWhiteSpace(telephone)) Telephone = telephone;
        if (!string.IsNullOrWhiteSpace(adresse)) Adresse = adresse;
    }

    public override string ToString() => $"Client {IdClient} : {Nom}";
}
