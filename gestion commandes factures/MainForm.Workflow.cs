using Microsoft.EntityFrameworkCore;

public sealed partial class MainForm
{
    private readonly List<Devis> _devis = new();
    private readonly List<BonCommande> _commandes = new();
    private readonly List<Facture> _factures = new();
    private int _prochainDevisId = 1;
    private int _prochaineCommandeId = 1;
    private int _prochaineFactureId = 1;

    private readonly DataGridView _devisGrid = new();
    private readonly ComboBox _devisClient = new();
    private readonly ComboBox _devisProduit = new();
    private readonly DateTimePicker _devisValidite = new();
    private readonly NumericUpDown _devisQuantite = new();

    private readonly DataGridView _commandesGrid = new();
    private readonly DateTimePicker _factureEcheance = new();
    private readonly DataGridView _facturesGrid = new();
    private readonly TextBox _paiementMontant = new();
    private readonly ComboBox _paiementMode = new();
    private readonly DataGridView _paiementsGrid = new();
    private readonly DataGridView _relancesGrid = new();

    private void AjouterOngletsMetier(TabControl tabs)
    {
        tabs.TabPages.Add(CreerOngletDevis());
        tabs.TabPages.Add(CreerOngletCommandes());
        tabs.TabPages.Add(CreerOngletFactures());
        tabs.TabPages.Add(CreerOngletPaiements());
        tabs.TabPages.Add(CreerOngletRelances());
    }

    private TabPage CreerOngletDevis()
    {
        var page = new TabPage("Devis");
        var layout = CreerLayoutPrincipal();
        ConfigurerGridMetier(_devisGrid);
        layout.Controls.Add(_devisGrid, 0, 0);

        _devisValidite.Value = DateTime.Today.AddDays(15);
        _devisQuantite.Minimum = 1;
        _devisQuantite.Maximum = 100000;
        _devisQuantite.Value = 1;
        var formulaire = CreerBarreFormulaire(
            Champ("Client", _devisClient),
            Champ("Produit", _devisProduit),
            Champ("Valide jusqu'au", _devisValidite),
            Champ("Quantité", _devisQuantite),
            Boutons(
                ("Créer le devis", CreerDevis),
                ("Ajouter une ligne", AjouterLigneDevis),
                ("Accepter", AccepterDevis),
                ("Refuser", RefuserDevis),
                ("Convertir en commande", ConvertirDevis)));
        layout.Controls.Add(formulaire, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreerOngletCommandes()
    {
        var page = new TabPage("Commandes");
        var layout = CreerLayoutPrincipal();
        ConfigurerGridMetier(_commandesGrid);
        layout.Controls.Add(_commandesGrid, 0, 0);

        _factureEcheance.Value = DateTime.Today.AddDays(30);
        var formulaire = CreerBarreFormulaire(
            Champ("Échéance facture", _factureEcheance),
            Boutons(
                ("Valider", ValiderCommande),
                ("Annuler", AnnulerCommande),
                ("Générer la facture", GenererFacture)));
        layout.Controls.Add(formulaire, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreerOngletFactures()
    {
        var page = new TabPage("Factures");
        var layout = CreerLayoutPrincipal();
        ConfigurerGridMetier(_facturesGrid);
        layout.Controls.Add(_facturesGrid, 0, 0);

        _paiementMode.DataSource = Enum.GetValues<ModePaiement>();
        _paiementMontant.Width = 100;
        var formulaire = CreerBarreFormulaire(
            Champ("Montant", _paiementMontant),
            Champ("Mode", _paiementMode),
            Boutons(
                ("Enregistrer paiement", EnregistrerPaiement),
                ("Créer une relance", CreerRelance)));
        layout.Controls.Add(formulaire, 0, 1);
        page.Controls.Add(layout);
        _facturesGrid.SelectionChanged += (_, _) => ActualiserPaiementsEtRelances();
        return page;
    }

    private TabPage CreerOngletPaiements()
    {
        var page = new TabPage("Paiements");
        ConfigurerGridMetier(_paiementsGrid);
        page.Controls.Add(_paiementsGrid);
        return page;
    }

    private TabPage CreerOngletRelances()
    {
        var page = new TabPage("Relances");
        ConfigurerGridMetier(_relancesGrid);
        page.Controls.Add(_relancesGrid);
        return page;
    }

    private static void ConfigurerGridMetier(DataGridView grid)
    {
        ConfigurerGrid(grid);
        grid.AutoGenerateColumns = true;
    }

    private static Control CreerBarreFormulaire(params Control[] controles)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(4)
        };
        panel.Controls.AddRange(controles);
        return panel;
    }

    private static Control Champ(string nom, Control controle)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, Margin = new Padding(4) };
        panel.Controls.Add(new Label { Text = nom, AutoSize = true });
        controle.Width = Math.Max(controle.Width, 150);
        panel.Controls.Add(controle);
        return panel;
    }

    private static Control Boutons(params (string Texte, EventHandler Action)[] boutons)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(4, 20, 4, 4) };
        foreach (var (texte, action) in boutons)
        {
            var bouton = new Button { Text = texte, AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
            bouton.Click += action;
            panel.Controls.Add(bouton);
        }
        return panel;
    }

    private void ChargerDonneesMetier()
    {
        using var context = new AppDbContext();
        _devisClient.DataSource = context.Clients.AsNoTracking().OrderBy(c => c.Nom).ToList();
        _devisClient.DisplayMember = nameof(Client.Nom);
        _devisClient.ValueMember = nameof(Client.IdClient);
        _devisProduit.DataSource = context.Produits.AsNoTracking().OrderBy(p => p.Designation).ToList();
        _devisProduit.DisplayMember = nameof(Produit.Designation);
        _devisProduit.ValueMember = nameof(Produit.IdProduit);
        RafraichirGrillesMetier();
    }

    private void CreerDevis(object? sender, EventArgs e)
    {
        if (_devisClient.SelectedItem is not Client client)
        {
            MessageBox.Show("Ajoutez d'abord un client.");
            return;
        }
        var devis = new Devis(_prochainDevisId++, client, _devisValidite.Value.Date);
        _devis.Add(devis);
        RafraichirGrillesMetier();
        SelectionnerDerniereLigne(_devisGrid);
        MessageBox.Show("Devis créé. Sélectionnez-le puis ajoutez des lignes.");
    }

    private void AjouterLigneDevis(object? sender, EventArgs e)
    {
        var devis = ElementSelectionne(_devisGrid, _devis);
        if (devis is null || _devisProduit.SelectedItem is not Produit produit)
        {
            MessageBox.Show("Sélectionnez un devis et un produit.");
            return;
        }
        try
        {
            devis.AjouterLigne(produit, (int)_devisQuantite.Value);
            RafraichirGrillesMetier();
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void AccepterDevis(object? sender, EventArgs e)
    {
        var devis = ElementSelectionne(_devisGrid, _devis);
        if (devis is null) return;
        try { devis.Accepter(); RafraichirGrillesMetier(); }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void RefuserDevis(object? sender, EventArgs e)
    {
        var devis = ElementSelectionne(_devisGrid, _devis);
        if (devis is null) return;
        devis.Refuser();
        RafraichirGrillesMetier();
    }

    private void ConvertirDevis(object? sender, EventArgs e)
    {
        var devis = ElementSelectionne(_devisGrid, _devis);
        if (devis is null) return;
        try
        {
            var commande = devis.ConvertirEnCommande(_prochaineCommandeId++);
            _commandes.Add(commande);
            RafraichirGrillesMetier();
            MessageBox.Show("Le devis a été converti en commande.");
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void ValiderCommande(object? sender, EventArgs e)
    {
        var commande = ElementSelectionne(_commandesGrid, _commandes);
        if (commande is null) return;
        try { commande.Valider(); RafraichirGrillesMetier(); ChargerProduits(); }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void AnnulerCommande(object? sender, EventArgs e)
    {
        var commande = ElementSelectionne(_commandesGrid, _commandes);
        if (commande is null) return;
        commande.Annuler();
        RafraichirGrillesMetier();
    }

    private void GenererFacture(object? sender, EventArgs e)
    {
        var commande = ElementSelectionne(_commandesGrid, _commandes);
        if (commande is null) return;
        try
        {
            var facture = commande.GenererFacture(_prochaineFactureId++, _factureEcheance.Value.Date);
            _factures.Add(facture);
            RafraichirGrillesMetier();
            MessageBox.Show("Facture générée avec succès.");
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void EnregistrerPaiement(object? sender, EventArgs e)
    {
        var facture = ElementSelectionne(_facturesGrid, _factures);
        if (facture is null) return;
        if (!decimal.TryParse(_paiementMontant.Text, out var montant) || _paiementMode.SelectedItem is not ModePaiement mode)
        {
            MessageBox.Show("Saisissez un montant valide et choisissez un mode de paiement.");
            return;
        }
        try
        {
            facture.EnregistrerPaiement(montant, mode);
            _paiementMontant.Clear();
            RafraichirGrillesMetier();
        }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void CreerRelance(object? sender, EventArgs e)
    {
        var facture = ElementSelectionne(_facturesGrid, _factures);
        if (facture is null) return;
        try { facture.Relancer(); RafraichirGrillesMetier(); }
        catch (Exception ex) { AfficherErreur(ex); }
    }

    private void RafraichirGrillesMetier()
    {
        _devisGrid.DataSource = _devis.Select(d => new DevisRow(d)).ToList();
        CacherColonne(_devisGrid, nameof(DevisRow.Objet));
        _commandesGrid.DataSource = _commandes.Select(c => new CommandeRow(c)).ToList();
        CacherColonne(_commandesGrid, nameof(CommandeRow.Objet));
        _facturesGrid.DataSource = _factures.Select(f => new FactureRow(f)).ToList();
        CacherColonne(_facturesGrid, nameof(FactureRow.Objet));
        ActualiserPaiementsEtRelances();
    }

    private void ActualiserPaiementsEtRelances()
    {
        var facture = ElementSelectionne(_facturesGrid, _factures);
        _paiementsGrid.DataSource = facture?.Paiements.ToList() ?? new List<Paiement>();
        _relancesGrid.DataSource = facture?.Relances.ToList() ?? new List<Relance>();
    }

    private static void CacherColonne(DataGridView grid, string nom)
    {
        if (grid.Columns.Contains(nom))
            grid.Columns[nom].Visible = false;
    }

    private static T? ElementSelectionne<T>(DataGridView grid, List<T> elements) where T : class
    {
        var index = grid.CurrentRow?.Index ?? -1;
        return index >= 0 && index < elements.Count ? elements[index] : null;
    }

    private static void SelectionnerDerniereLigne(DataGridView grid)
    {
        if (grid.Rows.Count > 0)
            grid.CurrentCell = grid.Rows[^1].Cells[0];
    }

    private sealed class DevisRow
    {
        public DevisRow(Devis devis) => Objet = devis;
        public Devis Objet { get; }
        public int Id => Objet.IdDevis;
        public string Client => Objet.Client.Nom;
        public DateTime Date => Objet.Date;
        public DateTime Validite => Objet.DateValidite;
        public string Statut => Objet.Statut.ToString();
        public decimal TotalHT => Objet.CalculerTotal();
        public int NombreLignes => Objet.Lignes.Count;
    }

    private sealed class CommandeRow
    {
        public CommandeRow(BonCommande commande) => Objet = commande;
        public BonCommande Objet { get; }
        public int Id => Objet.IdCommande;
        public string Client => Objet.Client.Nom;
        public DateTime Date => Objet.Date;
        public string Statut => Objet.Statut.ToString();
        public decimal TotalHT => Objet.CalculerTotal();
    }

    private sealed class FactureRow
    {
        public FactureRow(Facture facture) => Objet = facture;
        public Facture Objet { get; }
        public int Id => Objet.IdFacture;
        public string Client => Objet.Commande.Client.Nom;
        public decimal MontantHT => Objet.MontantHT;
        public decimal MontantTTC => Objet.CalculerTTC();
        public decimal Reste => Objet.ResteAPayer();
        public DateTime Echeance => Objet.DateEcheance;
        public string Statut => Objet.Statut.ToString();
    }
}
