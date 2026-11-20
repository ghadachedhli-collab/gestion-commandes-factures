using Microsoft.EntityFrameworkCore;

using (var context = new AppDbContext())
{
    context.Database.Migrate();
}

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Gestion commandes et factures ===");
    Console.WriteLine("1. Afficher les clients");
    Console.WriteLine("2. Ajouter un client");
    Console.WriteLine("3. Modifier un client");
    Console.WriteLine("4. Supprimer un client");
    Console.WriteLine("5. Exécuter la démonstration métier");
    Console.WriteLine("0. Quitter");
    Console.Write("Votre choix : ");

    switch (Console.ReadLine())
    {
        case "1": AfficherClients(); break;
        case "2": AjouterClient(); break;
        case "3": ModifierClient(); break;
        case "4": SupprimerClient(); break;
        case "5": ExecuterDemonstration(); break;
        case "0": return;
        default: Pause("Choix invalide."); break;
    }
}

static void AfficherClients()
{
    using var context = new AppDbContext();
    var clients = context.Clients.AsNoTracking().OrderBy(c => c.Nom).ToList();

    Console.WriteLine("\n=== Clients ===");
    if (clients.Count == 0)
    {
        Console.WriteLine("Aucun client enregistré.");
    }
    else
    {
        foreach (var client in clients)
            Console.WriteLine($"{client.IdClient} - {client.Nom} | {client.Email} | {client.Telephone} | {client.Adresse}");
    }

    Pause();
}

static void AjouterClient()
{
    Console.WriteLine("\n=== Ajouter un client ===");
    var client = new Client(
        0,
        LireTexte("Nom : "),
        LireTexte("Email : "),
        LireTexte("Téléphone : "),
        LireTexte("Adresse : "));

    using var context = new AppDbContext();
    context.Clients.Add(client);
    context.SaveChanges();
    Pause($"Client enregistré avec l'identifiant {client.IdClient}.");
}

static void ModifierClient()
{
    Console.WriteLine("\n=== Modifier un client ===");
    var id = LireEntier("Identifiant du client : ");

    using var context = new AppDbContext();
    var client = context.Clients.SingleOrDefault(c => c.IdClient == id);
    if (client is null)
    {
        Pause("Client introuvable.");
        return;
    }

    Console.WriteLine($"Laissez vide pour conserver la valeur actuelle ({client.Nom}).");
    var nom = LireOptionnel("Nom", client.Nom);
    var email = LireOptionnel("Email", client.Email);
    var telephone = LireOptionnel("Téléphone", client.Telephone);
    var adresse = LireOptionnel("Adresse", client.Adresse);

    client.ModifierInfos(nom, email, telephone, adresse);
    context.SaveChanges();
    Pause("Client modifié avec succès.");
}

static void SupprimerClient()
{
    Console.WriteLine("\n=== Supprimer un client ===");
    var id = LireEntier("Identifiant du client : ");

    using var context = new AppDbContext();
    var client = context.Clients.SingleOrDefault(c => c.IdClient == id);
    if (client is null)
    {
        Pause("Client introuvable.");
        return;
    }

    Console.Write($"Confirmer la suppression de {client.Nom} (o/n) : ");
    if (Console.ReadLine()?.Trim().ToLowerInvariant() != "o")
    {
        Pause("Suppression annulée.");
        return;
    }

    context.Clients.Remove(client);
    context.SaveChanges();
    Pause("Client supprimé avec succès.");
}

static void ExecuterDemonstration()
{
    var client = new Client(1, "Ali Ben Salah", "ali@mail.tn", "20123456", "Tunis");
    var ordinateur = new Produit(1, "Ordinateur", 1500m, 10);
    var souris = new Produit(2, "Souris", 30m, 50);

    var devis = new Devis(1, client, DateTime.Today.AddDays(15));
    devis.AjouterLigne(ordinateur, 2);
    devis.AjouterLigne(souris, 5);
    Console.WriteLine($"Total devis HT : {devis.CalculerTotal():F2} DT");

    devis.Accepter();
    var commande = devis.ConvertirEnCommande(1);
    commande.Valider();
    Console.WriteLine($"Stock ordinateurs après commande : {ordinateur.Stock}");

    var facture = commande.GenererFacture(1, DateTime.Today.AddDays(-1));
    Console.WriteLine($"Total TTC : {facture.CalculerTTC():F2} DT");
    facture.EnregistrerPaiement(1000m, ModePaiement.Virement);
    Console.WriteLine($"Reste à payer : {facture.ResteAPayer():F2} DT ({facture.Statut})");
    facture.Relancer();
    Console.WriteLine($"Statut : {facture.Statut}");
    facture.EnregistrerPaiement(facture.ResteAPayer(), ModePaiement.Carte);
    Console.WriteLine($"Statut final : {facture.Statut}");

    Pause();
}

static string LireTexte(string libelle)
{
    while (true)
    {
        Console.Write(libelle);
        var valeur = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(valeur))
            return valeur;
        Console.WriteLine("Cette valeur est obligatoire.");
    }
}

static string LireOptionnel(string libelle, string valeurActuelle)
{
    Console.Write($"{libelle} [{valeurActuelle}] : ");
    var valeur = Console.ReadLine()?.Trim();
    return string.IsNullOrWhiteSpace(valeur) ? valeurActuelle : valeur;
}

static int LireEntier(string libelle)
{
    while (true)
    {
        Console.Write(libelle);
        if (int.TryParse(Console.ReadLine(), out var valeur) && valeur > 0)
            return valeur;
        Console.WriteLine("Veuillez saisir un identifiant valide.");
    }
}

static void Pause(string? message = null)
{
    if (!string.IsNullOrWhiteSpace(message))
        Console.WriteLine(message);
    Console.WriteLine("Appuyez sur une touche pour continuer...");
    Console.ReadKey();
}