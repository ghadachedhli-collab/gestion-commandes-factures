// Données de départ
var client = new Client(1, "Ali Ben Salah", "ali@mail.tn", "20123456", "Tunis");
var ordinateur = new Produit(1, "Ordinateur", 1500m, 10);
var souris = new Produit(2, "Souris", 30m, 50);

// 1) Devis
var devis = new Devis(1, client, DateTime.Today.AddDays(15));
devis.AjouterLigne(ordinateur, 2);
devis.AjouterLigne(souris, 5);
Console.WriteLine($"Total devis HT : {devis.CalculerTotal():F2} DT");

// 2) Acceptation puis bon de commande
devis.Accepter();
var commande = devis.ConvertirEnCommande(1);
commande.Valider();
Console.WriteLine($"Stock ordinateurs après commande : {ordinateur.Stock}");

// 3) Facture (échéance déjà dépassée pour tester la relance)
var facture = commande.GenererFacture(1, DateTime.Today.AddDays(-1));
Console.WriteLine($"Total TTC : {facture.CalculerTTC():F2} DT");

// 4) Paiement partiel
facture.EnregistrerPaiement(1000m, ModePaiement.Virement);
Console.WriteLine($"Reste à payer : {facture.ResteAPayer():F2} DT ({facture.Statut})");

// 5) Relance
facture.Relancer();
Console.WriteLine($"Statut : {facture.Statut}");

// 6) Paiement du solde
facture.EnregistrerPaiement(facture.ResteAPayer(), ModePaiement.Carte);
Console.WriteLine($"Statut final : {facture.Statut}");
