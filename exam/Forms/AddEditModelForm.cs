using exam.Data;
using exam.Models;

namespace exam.Forms;

public sealed partial class AddEditModelForm : Form
{
    private readonly TBaseModelRepository _repository;
    private readonly bool _isEdit;
    private readonly TBaseModel _original;

    private readonly TextBox txtFactoryCode;
    private readonly TextBox txtModel;
    private readonly TextBox txtModelDesc;
    private readonly ComboBox cmbColor;
    private readonly TextBox txtImeiCount;
    private readonly TextBox txtBoxNumber;
    private readonly TextBox txtNote;
    private readonly Button btnSave;
    private readonly Button btnCancel;

    public TBaseModel? Result { get; private set; }

    public AddEditModelForm(string connectionString, string auditUser, TBaseModel? modelToEdit = null)
    {
        _repository = new TBaseModelRepository(connectionString, auditUser);
        _isEdit = modelToEdit is not null;
        _original = modelToEdit ?? new TBaseModel();

        Text = _isEdit ? "Edit Model" : "Add Model";
        ClientSize = new Size(600, 620);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.FromArgb(0, 122, 204)
        };
        var lblTitle = new Label
        {
            Text = _isEdit ? "Edit Model" : "Add Model",
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Location = new Point(20, 12),
            Size = new Size(560, 32)
        };
        header.Controls.Add(lblTitle);

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(24, 20, 24, 20)
        };

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(0, 0, 0, 20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var row in Enumerable.Range(0, 9))
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles[8] = new RowStyle(SizeType.Percent, 100);

        txtFactoryCode = CreateTextBox(_original.FactoryCode, readOnly: _isEdit);
        txtModel = CreateTextBox(_original.Model, readOnly: _isEdit);
        txtModelDesc = CreateTextBox(_original.ModelDesc);
        cmbColor = CreateColorCombo(_original.Color);
        txtImeiCount = CreateTextBox(_original.ImeiCount.HasValue ? _original.ImeiCount.Value.ToString() : (_isEdit ? string.Empty : "2"));
        txtBoxNumber = CreateTextBox(_original.BoxNumber.HasValue ? _original.BoxNumber.Value.ToString() : (_isEdit ? string.Empty : "10"));
        txtNote = CreateTextBox(_original.Note, multiline: true);

        layout.Controls.Add(StyledLabel("Factory code *"), 0, 0);
        layout.Controls.Add(txtFactoryCode, 1, 0);
        layout.Controls.Add(StyledLabel("Model *"), 0, 1);
        layout.Controls.Add(txtModel, 1, 1);
        layout.Controls.Add(StyledLabel("Description"), 0, 2);
        layout.Controls.Add(txtModelDesc, 1, 2);
        layout.Controls.Add(StyledLabel("Color"), 0, 3);
        layout.Controls.Add(cmbColor, 1, 3);
        layout.Controls.Add(StyledLabel("IMEI count"), 0, 4);
        layout.Controls.Add(txtImeiCount, 1, 4);
        layout.Controls.Add(StyledLabel("Units per box"), 0, 5);
        layout.Controls.Add(txtBoxNumber, 1, 5);
        layout.Controls.Add(StyledLabel("Notes"), 0, 6);
        layout.Controls.Add(txtNote, 1, 6);

        var noteHint = new Label
        {
            Text = "Leave empty to store NULL (edit mode only).",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 6, 0, 0),
            ForeColor = Color.FromArgb(120, 120, 120),
            Font = new Font("Segoe UI", 8f, FontStyle.Italic)
        };
        layout.Controls.Add(noteHint, 1, 7);
        layout.SetColumnSpan(noteHint, 1);

        var empty = new Label { Dock = DockStyle.Fill };
        layout.Controls.Add(empty, 0, 8);
        layout.SetColumnSpan(empty, 2);

        scroll.Controls.Add(layout);

        btnSave = new Button
        {
            Text = "Save",
            Width = 130,
            Height = 40,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "Cancel",
            Width = 130,
            Height = 40,
            BackColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.FromArgb(80, 80, 80),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10f),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.DialogResult = DialogResult.Cancel;

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            Padding = new Padding(24, 12, 24, 12),
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.FromArgb(248, 248, 248)
        };
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        Controls.Add(scroll);
        Controls.Add(header);
        Controls.Add(buttonPanel);
    }

    private static Label StyledLabel(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Dock = DockStyle.Fill,
        Padding = new Padding(0, 12, 0, 0),
        ForeColor = Color.FromArgb(60, 60, 60),
        Font = new Font("Segoe UI", 9.5f),
        Anchor = AnchorStyles.Left | AnchorStyles.Top
    };

    private static TextBox CreateTextBox(string? text, bool multiline = false, bool readOnly = false)
    {
        var box = new TextBox
        {
            Text = text ?? string.Empty,
            Multiline = multiline,
            ReadOnly = readOnly,
            Dock = DockStyle.Fill,
            Height = multiline ? 80 : 32,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            Font = new Font("Segoe UI", 10f),
            BackColor = readOnly ? Color.FromArgb(245, 245, 245) : Color.White
        };
        return box;
    }

    private ComboBox CreateColorCombo(string? currentColor)
    {
        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            MaxDropDownItems = 12,
            Font = new Font("Segoe UI", 10f),
            Height = 32
        };

        try
        {
            var colors = _repository.GetDistinctColors();
            foreach (var color in colors)
                combo.Items.Add(color);
        }
        catch
        {
        }

        if (!string.IsNullOrWhiteSpace(currentColor))
        {
            combo.Text = currentColor;
            if (!combo.Items.Contains(currentColor))
                combo.Items.Add(currentColor);
        }

        return combo;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            var model = new TBaseModel
            {
                FactoryCode = txtFactoryCode.Text,
                Model = txtModel.Text,
                ModelDesc = string.IsNullOrWhiteSpace(txtModelDesc.Text) ? null : txtModelDesc.Text,
                Color = string.IsNullOrWhiteSpace(cmbColor.Text) ? null : cmbColor.Text.Trim(),
                ImeiCount = ParsePositiveInt(txtImeiCount.Text, "IMEI count"),
                BoxNumber = ParsePositiveInt(txtBoxNumber.Text, "Units per box"),
                Note = string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text
            };

            if (_isEdit)
            {
                model.Id = _original.Id;
                _repository.Update(model);
            }
            else
            {
                _repository.Insert(model);
            }

            Result = model;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Cursor.Current = Cursors.Default;
            DialogResult = DialogResult.None;
            MessageBox.Show(ex.Message, _isEdit ? "Update failed" : "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    private static int? ParsePositiveInt(string text, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (!int.TryParse(text.Trim(), out var value))
            throw new ModelValidationException($"{fieldName} must be a valid integer.");
        if (value <= 0)
            throw new ModelValidationException($"{fieldName} must be a positive integer.");
        return value;
    }
}
