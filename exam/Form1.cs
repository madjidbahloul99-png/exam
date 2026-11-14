using System.ComponentModel;
using System.Text.Json;
using exam.Data;
using exam.Forms;
using exam.Models;

namespace exam;

public sealed partial class Form1 : Form
{
    private readonly string _connectionString;
    private readonly string _auditUser;
    private readonly BindingList<TBaseModel> _records = new();

    private TextBox txtFactoryCode;
    private TextBox txtModel;
    private DataGridView dgvModels;
    private Label lblInfo;

    public Form1()
    {
        (_connectionString, _auditUser) = LoadConfiguration();

        Text = "Model Management";
        ClientSize = new Size(1600, 850);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 600);
        BackColor = Color.White;

        var header = BuildHeader();
        var panelTop = BuildTopPanel();
        dgvModels = BuildGrid();

        header.Dock = DockStyle.Top;
        header.Height = 60;
        panelTop.Dock = DockStyle.Top;
        panelTop.Height = 100;
        dgvModels.Dock = DockStyle.Fill;

        Controls.Add(dgvModels);
        Controls.Add(panelTop);
        Controls.Add(header);

        Load += Form1_Load;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        RefreshGrid();
    }

    private Panel BuildHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(0, 122, 204)
        };

        var lblTitle = new Label
        {
            Text = "Model Management",
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            Location = new Point(24, 15),
            Size = new Size(400, 32)
        };

        panel.Controls.Add(lblTitle);
        return panel;
    }

    private Panel BuildTopPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(248, 248, 248),
            Padding = new Padding(24, 12, 24, 12)
        };

        // Search row
        var lblFactoryCode = StyledLabel("Factory code");
        txtFactoryCode = new TextBox
        {
            Width = 200,
            Height = 32,
            Font = new Font("Segoe UI", 10f)
        };
        txtFactoryCode.KeyDown += Search_KeyDown;

        var lblModel = StyledLabel("Model (contains)");
        txtModel = new TextBox
        {
            Width = 240,
            Height = 32,
            Font = new Font("Segoe UI", 10f)
        };
        txtModel.KeyDown += Search_KeyDown;

        var btnSearch = StyledButton("Search", true);
        btnSearch.Click += BtnSearch_Click;

        var btnReset = StyledButton("Reset", false);
        btnReset.Click += BtnReset_Click;

        lblInfo = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(100, 100, 100),
            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
            Dock = DockStyle.Right,
            Width = 260
        };

        var flowSearch = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 0)
        };
        flowSearch.Controls.Add(lblFactoryCode);
        flowSearch.Controls.Add(txtFactoryCode);
        flowSearch.Controls.Add(lblModel);
        flowSearch.Controls.Add(txtModel);
        flowSearch.Controls.Add(btnSearch);
        flowSearch.Controls.Add(btnReset);
        flowSearch.Controls.Add(new Label { AutoSize = false, Width = 20 });

        // Action row (all buttons at top)
        var btnAdd = StyledButton("Add", true);
        btnAdd.Click += BtnAdd_Click;

        var btnEdit = StyledButton("Edit", false);
        btnEdit.Click += BtnEdit_Click;

        var btnDelete = StyledButton("Delete", false);
        btnDelete.Click += BtnDelete_Click;

        var flowActions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 10, 0, 0)
        };
        flowActions.Controls.Add(btnAdd);
        flowActions.Controls.Add(btnEdit);
        flowActions.Controls.Add(btnDelete);

        panel.Controls.Add(flowSearch);
        panel.Controls.Add(flowActions);
        panel.Controls.Add(lblInfo);

        lblInfo.Location = new Point(900, 40);
        lblInfo.Size = new Size(260, 24);

        return panel;
    }

    private static Label StyledLabel(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Height = 24,
        Width = 130,
        ForeColor = Color.FromArgb(60, 60, 60),
        Font = new Font("Segoe UI", 9.5f),
        Anchor = AnchorStyles.Left
    };

    private static Button StyledButton(string text, bool primary)
    {
        var btn = new Button
        {
            Text = text,
            Width = 110,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f),
            Cursor = Cursors.Hand
        };
        if (primary)
        {
            btn.BackColor = Color.FromArgb(0, 122, 204);
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }
        else
        {
            btn.BackColor = Color.FromArgb(240, 240, 240);
            btn.ForeColor = Color.FromArgb(60, 60, 60);
            btn.FlatAppearance.BorderSize = 0;
        }
        return btn;
    }

    private DataGridView BuildGrid()
    {
        var grid = new DataGridView
        {
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            RowHeadersVisible = false,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Color.White,
            GridColor = Color.FromArgb(230, 230, 230),
            Font = new Font("Segoe UI", 9.5f),
            RowTemplate = { Height = 34 },
            ColumnHeadersHeight = 40,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 251, 252)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 40, 40)
            }
        };

        grid.Columns.AddRange(new DataGridViewColumn[]
        {
            TextColumn("id", "Id", width: 80, visible: false),
            TextColumn("factoryCode", "Factory code", width: 160),
            TextColumn("model", "Model", width: 180),
            TextColumn("modelDesc", "Description", width: 260),
            TextColumn("color", "Color", width: 140),
            TextColumn("imeiCount", "IMEI count", width: 110, align: DataGridViewContentAlignment.MiddleRight),
            TextColumn("boxNumber", "Units per box", width: 140, align: DataGridViewContentAlignment.MiddleRight),
            TextColumn("note", "Notes", width: 300),
            DateColumn("createDate", "Created on", width: 180),
            TextColumn("createUser", "Created by", width: 150),
            DateColumn("modifyDate", "Modified on", width: 180),
            TextColumn("modifyUser", "Modified by", width: 150)
        });

        grid.DataSource = _records;
        return grid;
    }

    private static DataGridViewTextBoxColumn TextColumn(string dataProperty, string header, int width = 100, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft, bool visible = true)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = dataProperty,
            HeaderText = header,
            Width = width,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = align },
            Visible = visible
        };
        return column;
    }

    private static DataGridViewTextBoxColumn DateColumn(string dataProperty, string header, int width = 100)
    {
        var column = TextColumn(dataProperty, header, width);
        column.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
        column.DefaultCellStyle.NullValue = string.Empty;
        return column;
    }

    private void Search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnSearch_Click(sender, e);
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        RefreshGrid(txtFactoryCode.Text, txtModel.Text);
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        txtFactoryCode.Clear();
        txtModel.Clear();
        RefreshGrid();
    }

    private void RefreshGrid(string? factoryCode = null, string? model = null)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            using var repository = new TBaseModelRepository(_connectionString, _auditUser);
            var list = repository.Get(factoryCode ?? string.Empty, model ?? string.Empty);

            _records.RaiseListChangedEvents = false;
            _records.Clear();
            foreach (var record in list)
                _records.Add(record);
            _records.RaiseListChangedEvents = true;
            _records.ResetBindings();

            var count = _records.Count;
            lblInfo.Text = count == 0 ? "No records match." : $"{count} record(s)";
            lblInfo.Visible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load records: {ex.Message}", "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var form = new AddEditModelForm(_connectionString, _auditUser);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            RefreshGrid(txtFactoryCode.Text, txtModel.Text);
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        var selected = dgvModels.CurrentRow?.DataBoundItem as TBaseModel;
        if (selected is null)
        {
            MessageBox.Show("Select a record to edit.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var repository = new TBaseModelRepository(_connectionString, _auditUser);
        TBaseModel current;
        try
        {
            current = repository.GetByKey(selected.FactoryCode, selected.Model);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load record: {ex.Message}", "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var form = new AddEditModelForm(_connectionString, _auditUser, current);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            RefreshGrid(txtFactoryCode.Text, txtModel.Text);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        var selected = dgvModels.CurrentRow?.DataBoundItem as TBaseModel;
        if (selected is null)
        {
            MessageBox.Show("Select a record to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete the model '{selected.Model}' for factory '{selected.FactoryCode}'?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
            return;

        try
        {
            using var repository = new TBaseModelRepository(_connectionString, _auditUser);
            repository.Delete(selected.FactoryCode, selected.Model);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Delete failed: {ex.Message}", "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        RefreshGrid(txtFactoryCode.Text, txtModel.Text);
    }

    private static (string ConnectionString, string AuditUser) LoadConfiguration()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
            throw new FileNotFoundException("appsettings.json not found next to the application executable.", configPath);

        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        var root = document.RootElement;

        string connectionString;
        if (root.TryGetProperty("ConnectionStrings", out var connections) &&
            connections.TryGetProperty("ExamDb", out var connection))
        {
            connectionString = connection.GetString() ?? string.Empty;
        }
        else
        {
            connectionString = root.GetProperty("ConnectionString").GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'ExamDb' is not configured in appsettings.json.");

        var auditUser = root.TryGetProperty("AuditUser", out var user) ? user.GetString() ?? "exam_user" : "exam_user";
        return (connectionString, auditUser);
    }
}
