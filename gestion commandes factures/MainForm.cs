using System.Globalization;
using Microsoft.EntityFrameworkCore;

public sealed partial class MainForm : Form
{
    private readonly DataGridView _clientsGrid = new();
    private readonly TextBox _clientNom = new();
    private readonly TextBox _clientEmail = new();
    private readonly TextBox _clientTelephone = new();
    private readonly TextBox _clientAdresse = new();
    private readonly TextBox _clientRecherche = new();

    private readonly DataGridView _produitsGrid = new();
    private readonly TextBox _produitDesignation = new();
    private readonly TextBox _produitPrix = new();
    private readonly TextBox _produitStock = new();

    public MainForm()
    {
        Text = "Gestion des commandes et factures";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1100, 700);
        Font = new Font("Segoe UI", 10F);

        using (var context = new AppDbContext())
            context.Database.Migrate();

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreerOngletClients());
        tabs.TabPages.Add(CreerOngletProduits());
        AjouterOngletsMetier(tabs);
        Controls.Add(tabs);

        Load += (_, _) =>
        {
            InitialiserExemplesBase();
            ChargerClients();
            ChargerProduits();
            ChargerDonneesMetier();
        };
    }

    private TabPage CreerOngletClients()
    {
        var page = new TabPage("Clients");
        var layout = CreerLayoutPrincipal();
        var pageLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(12),
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 42),
                new RowStyle(SizeType.Percent, 100)
            }
        };
        var recherche = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        recherche.Controls.Add(new Label { Text = "Rechercher par nom :", AutoSize = true, Margin = new Padding(3, 8, 8, 3) });
        _clientRecherche.Width = 300;
        _clientRecherche.PlaceholderText = "Saisissez le nom du client";
        _clientRecherche.TextChanged += (_, _) => ChargerClients(_clientRecherche.Text);
        recherche.Controls.Add(_clientRecherche);

        ConfigurerClientsGrid();
        layout.Controls.Add(_clientsGrid, 0, 0);
        layout.Controls.Add(CreerFormulaireClient(), 0, 1);
        pageLayout.Controls.Add(recherche, 0, 0);
        pageLayout.Controls.Add(layout, 0, 1);
        page.Controls.Add(pageLayout);
        _clientsGrid.SelectionChanged += (_, _) => RemplirClientSelectionne();
        return page;
    }

    private TabPage CreerOngletProduits()
    {
        var page = new TabPage("Produits");
        var layout = CreerLayoutPrincipal();

        ConfigurerProduitsGrid();
        layout.Controls.Add(_produitsGrid, 0, 0);
        layout.Controls.Add(CreerFormulaireProduit(), 0, 1);
        page.Controls.Add(layout);
        _produitsGrid.SelectionChanged += (_, _) => RemplirProduitSelectionne();
        return page;
    }

    private static TableLayoutPanel CreerLayoutPrincipal() => new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(12),
        RowCount = 2,
        ColumnCount = 1,
        RowStyles =
        {
            new RowStyle(SizeType.Percent, 65),
            new RowStyle(SizeType.Percent, 35)
        }
    };

    private void ConfigurerClientsGrid()
    {
        ConfigurerGrid(_clientsGrid);
        AjouterColonne(_clientsGrid, "IdClient", "ID", 60);
        AjouterColonne(_clientsGrid, "Nom", "Nom", 180);
        AjouterColonne(_clientsGrid, "Email", "Email", 220);
        AjouterColonne(_clientsGrid, "Telephone", "Téléphone", 140);
        AjouterColonne(_clientsGrid, "Adresse", "Adresse", 220);
    }

    private void ConfigurerProduitsGrid()
    {
        ConfigurerGrid(_produitsGrid);
        AjouterColonne(_produitsGrid, "IdProduit", "ID", 60);
        AjouterColonne(_produitsGrid, "Designation", "Désignation", 250);
        AjouterColonne(_produitsGrid, "PrixUnitaire", "Prix unitaire", 140);
        AjouterColonne(_produitsGrid, "Stock", "Stock", 100);
    }

    private static void ConfigurerGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoGenerateColumns = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = SystemColors.Window;
    }

    private static void AjouterColonne(DataGridView grid, string property, string title, int width)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = title,
            Name = property,
            FillWeight = width
        });
    }

    private Control CreerFormulaireClient()
    {
        var layout = CreerFormulaireLayout(5, 2);
        AjouterChamp(layout, "Nom", _clientNom, 0);
        AjouterChamp(layout, "Email", _clientEmail, 1);
        AjouterChamp(layout, "Téléphone", _clientTelephone, 2);
        AjouterChamp(layout, "Adresse", _clientAdresse, 3);
        layout.Controls.Add(CreerBoutons(
            ("Ajouter", AjouterClient),
            ("Modifier", ModifierClient),
            ("Supprimer", SupprimerClient),
            ("Vider", ViderClient)), 0, 4);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 4)!, 2);
        return layout;
    }

    private Control CreerFormulaireProduit()
    {
        var layout = CreerFormulaireLayout(4, 2);
        AjouterChamp(layout, "Désignation", _produitDesignation, 0);
        AjouterChamp(layout, "Prix unitaire", _produitPrix, 1);
        AjouterChamp(layout, "Stock", _produitStock, 2);
        layout.Controls.Add(CreerBoutons(
            ("Ajouter", AjouterProduit),
            ("Modifier", ModifierProduit),
            ("Supprimer", SupprimerProduit),
            ("Vider", ViderProduit)), 0, 3);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 3)!, 2);
        return layout;
    }

    private static TableLayoutPanel CreerFormulaireLayout(int rows, int columns) => new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = columns,
        RowCount = rows,
        Padding = new Padding(4),
        ColumnStyles =
        {
            new ColumnStyle(SizeType.Absolute, 115),
            new ColumnStyle(SizeType.Percent, 100)
        }
    };

    private static void AjouterChamp(TableLayoutPanel layout, string label, Control input, int row)
    {
        layout.Controls.Add(new Label
        {
            Text = label + " :",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 8, 3, 3)
        }, 0, row);
        input.Dock = DockStyle.Fill;
        layout.Controls.Add(input, 1, row);
    }

    private static FlowLayoutPanel CreerBoutons(params (string Texte, EventHandler Action)[] boutons)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        foreach (var (texte, action) in boutons)
        {
            var button = new Button { Text = texte, AutoSize = true, Padding = new Padding(10, 4, 10, 4) };
            button.Click += action;
            panel.Controls.Add(button);
        }
        return panel;
    }

    private void ChargerClients(string? recherche = null)
    {
        using var context = new AppDbContext();
        var requete = context.Clients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(recherche))
            requete = requete.Where(c => c.Nom.Contains(recherche.Trim()));
        _clientsGrid.DataSource = requete.OrderBy(c => c.Nom).ToList();
    }

    private void ChargerProduits()
    {
        using var context = new AppDbContext();
        _produitsGrid.DataSource = context.Produits.AsNoTracking().OrderBy(p => p.Designation).ToList();
    }

    private void RemplirClientSelectionne()
    {
        if (_clientsGrid.CurrentRow?.DataBoundItem is not Client client)
            return;
        _clientNom.Text = client.Nom;
        _clientEmail.Text = client.Email;
        _clientTelephone.Text = client.Telephone;
        _clientAdresse.Text = client.Adresse;
    }

    private void RemplirProduitSelectionne()
    {
        if (_produitsGrid.CurrentRow?.DataBoundItem is not Produit produit)
            return;
        _produitDesignation.Text = produit.Designation;
        _produitPrix.Text = produit.PrixUnitaire.ToString("0.00", CultureInfo.CurrentCulture);
        _produitStock.Text = produit.Stock.ToString(CultureInfo.CurrentCulture);
    }

    private void AjouterClient(object? sender, EventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            context.Clients.Add(new Client(0, TexteObligatoire(_clientNom, "Nom"),
                TexteObligatoire(_clientEmail, "Email"),
                TexteObligatoire(_clientTelephone, "Téléphone"),
                TexteObligatoire(_clientAdresse, "Adresse")));
            context.SaveChanges();
            ChargerClients();
            ViderClient();
            MessageBox.Show("Client ajouté avec succès.", "Clients", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void ModifierClient(object? sender, EventArgs e)
    {
        if (!TryGetClientId(out var id)) return;
        try
        {
            using var context = new AppDbContext();
            var client = context.Clients.SingleOrDefault(c => c.IdClient == id);
            if (client is null) { MessageBox.Show("Client introuvable."); return; }
            client.ModifierInfos(TexteObligatoire(_clientNom, "Nom"), TexteObligatoire(_clientEmail, "Email"),
                TexteObligatoire(_clientTelephone, "Téléphone"), TexteObligatoire(_clientAdresse, "Adresse"));
            context.SaveChanges();
            ChargerClients();
            MessageBox.Show("Client modifié avec succès.", "Clients", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void SupprimerClient(object? sender, EventArgs e)
    {
        if (!TryGetClientId(out var id)) return;
        if (MessageBox.Show("Supprimer le client sélectionné ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        try
        {
            using var context = new AppDbContext();
            var client = context.Clients.Find(id);
            if (client is null) return;
            context.Clients.Remove(client);
            context.SaveChanges();
            ChargerClients();
            ViderClient();
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void AjouterProduit(object? sender, EventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            context.Produits.Add(new Produit(0, TexteObligatoire(_produitDesignation, "Désignation"),
                LirePrix(), LireStock()));
            context.SaveChanges();
            ChargerProduits();
            ViderProduit();
            MessageBox.Show("Produit ajouté avec succès.", "Produits", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void ModifierProduit(object? sender, EventArgs e)
    {
        if (!TryGetProduitId(out var id)) return;
        try
        {
            using var context = new AppDbContext();
            var produit = context.Produits.SingleOrDefault(p => p.IdProduit == id);
            if (produit is null) { MessageBox.Show("Produit introuvable."); return; }
            produit.ModifierInfos(TexteObligatoire(_produitDesignation, "Désignation"), LirePrix(), LireStock());
            context.SaveChanges();
            ChargerProduits();
            MessageBox.Show("Produit modifié avec succès.", "Produits", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void SupprimerProduit(object? sender, EventArgs e)
    {
        if (!TryGetProduitId(out var id)) return;
        if (MessageBox.Show("Supprimer le produit sélectionné ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        try
        {
            using var context = new AppDbContext();
            var produit = context.Produits.Find(id);
            if (produit is null) return;
            context.Produits.Remove(produit);
            context.SaveChanges();
            ChargerProduits();
            ViderProduit();
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private bool TryGetClientId(out int id) => TryGetId(_clientsGrid, "client", "IdClient", out id);
    private bool TryGetProduitId(out int id) => TryGetId(_produitsGrid, "produit", "IdProduit", out id);

    private static bool TryGetId(DataGridView grid, string type, string property, out int id)
    {
        id = 0;
        if (grid.CurrentRow?.DataBoundItem is null || !int.TryParse(grid.CurrentRow.Cells[property].Value?.ToString(), out id))
        {
            MessageBox.Show($"Sélectionnez un {type}.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        return true;
    }

    private static string TexteObligatoire(TextBox textBox, string nom)
    {
        var valeur = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(valeur))
            throw new ArgumentException($"Le champ {nom} est obligatoire.");
        return valeur;
    }

    private decimal LirePrix()
    {
        if (!decimal.TryParse(_produitPrix.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var prix) || prix < 0)
            throw new ArgumentException("Le prix doit être un nombre positif.");
        return prix;
    }

    private int LireStock()
    {
        if (!int.TryParse(_produitStock.Text, out var stock) || stock < 0)
            throw new ArgumentException("Le stock doit être un nombre entier positif.");
        return stock;
    }

    private void ViderClient(object? sender = null, EventArgs? e = null)
    {
        _clientNom.Clear();
        _clientEmail.Clear();
        _clientTelephone.Clear();
        _clientAdresse.Clear();
        _clientsGrid.ClearSelection();
    }

    private void ViderProduit(object? sender = null, EventArgs? e = null)
    {
        _produitDesignation.Clear();
        _produitPrix.Clear();
        _produitStock.Clear();
        _produitsGrid.ClearSelection();
    }

    private static void AfficherErreur(Exception exception)
    {
        MessageBox.Show(exception.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
