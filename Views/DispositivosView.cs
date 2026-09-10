using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using momospos.Helpers;

namespace momospos.Views
{
    public class DispositivosView : UserControl
    {
        private DataGridView dgv;
        private Button btnNuevo, btnDesactivar, btnActivar, btnRefrescar;
        private Label lblConteo;

        public DispositivosView()
        {
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.BackgroundColor;

            bool small = Theme.IsSmallScreen();

            FlowLayoutPanel topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(15, small ? 12 : 18, 15, 10)
            };

            var lblTitulo = new Label
            {
                Text = "📱 Dispositivos Móviles",
                Font = new Font("Segoe UI", small ? 18 : 22, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Margin = new Padding(0, 0, 20, 0)
            };
            topPanel.Controls.Add(lblTitulo);

            int bw = small ? 120 : 150, bh = small ? 32 : 38;

            btnNuevo = new Button { Text = "➕ Nuevo", Width = bw, Height = bh, Margin = new Padding(5) };
            btnDesactivar = new Button { Text = "🚫 Desactivar", Width = bw, Height = bh, Margin = new Padding(5) };
            btnActivar = new Button { Text = "✅ Activar", Width = bw, Height = bh, Margin = new Padding(5) };
            btnRefrescar = new Button { Text = "🔄 Actualizar", Width = bw, Height = bh, Margin = new Padding(5) };

            Theme.StyleButton(btnNuevo, Theme.PrimaryColor, Color.White, Theme.FontNormal);
            Theme.StyleButton(btnDesactivar, Theme.DangerColor, Color.White, Theme.FontNormal);
            Theme.StyleButton(btnActivar, Color.FromArgb(39, 174, 96), Color.White, Theme.FontNormal);
            Theme.StyleButton(btnRefrescar, Theme.SecondaryColor, Color.White, Theme.FontNormal);

            btnNuevo.Click += BtnNuevo_Click;
            btnDesactivar.Click += (s, e) => ToggleActivo(false);
            btnActivar.Click += (s, e) => ToggleActivo(true);
            btnRefrescar.Click += (s, e) => CargarDatos();

            topPanel.Controls.Add(btnNuevo);
            topPanel.Controls.Add(btnActivar);
            topPanel.Controls.Add(btnDesactivar);
            topPanel.Controls.Add(btnRefrescar);

            lblConteo = new Label
            {
                Font = Theme.FontNormal, ForeColor = Color.Gray,
                AutoSize = true, Margin = new Padding(10, 10, 0, 0)
            };
            topPanel.Controls.Add(lblConteo);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 11),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 40
            };
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Theme.SecondaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgv.RowTemplate.Height = 38;

            this.Controls.Add(dgv);
            this.Controls.Add(topPanel);
        }

        private void CargarDatos()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",             HeaderText = "ID",               Width = 50,  FillWeight = 5 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre",         HeaderText = "Nombre",           FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Token",          HeaderText = "Token",            FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "PrefixFolio",   HeaderText = "Prefijo Folio",    FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Activo",        HeaderText = "Estado",           FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreadoEn",      HeaderText = "Creado",           FillWeight = 15 });

            try
            {
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "SELECT Id,Nombre,Token,PrefixFolio,Activo,CreadoEn FROM Dispositivos ORDER BY Id", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (r.Read())
                        {
                            count++;
                            bool activo = r.GetBoolean(4);
                            int idx = dgv.Rows.Add(
                                r.GetInt32(0),
                                r.GetString(1),
                                r.GetString(2),
                                r.GetString(3),
                                activo ? "✅ Activo" : "🚫 Inactivo",
                                r.GetDateTime(5).ToString("dd/MM/yyyy HH:mm")
                            );
                            dgv.Rows[idx].DefaultCellStyle.ForeColor = activo ? Color.DarkGreen : Color.Gray;
                            dgv.Rows[idx].Tag = r.GetInt32(0); // Id
                        }
                        lblConteo.Text = $"{count} dispositivo(s) registrado(s)";
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al cargar dispositivos:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new DispositivoForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    CargarDatos();
            }
        }

        private void ToggleActivo(bool activar)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show("Seleccione un dispositivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = (int)dgv.SelectedRows[0].Tag;
            string accion = activar ? "activar" : "desactivar";
            var res = CustomMessageBox.Show($"¿Desea {accion} este dispositivo?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            try
            {
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE Dispositivos SET Activo=@a WHERE Id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("a", activar);
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarDatos();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ─── Formulario Nuevo Dispositivo ────────────────────────────────────────
    public class DispositivoForm : Form
    {
        private TextBox txtNombre, txtPrefix;
        private Button btnGuardar, btnCancelar;
        private Label lblToken;

        public DispositivoForm()
        {
            this.Text = "Nuevo Dispositivo Móvil";
            this.Size = new Size(480, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            BuildUI();
        }

        private void BuildUI()
        {
            int y = 20;

            this.Controls.Add(new Label { Text = "Nombre del dispositivo:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            y += 28;
            txtNombre = new TextBox { Location = new Point(20, y), Width = 420, Font = new Font("Segoe UI", 13) };
            this.Controls.Add(txtNombre);
            y += 50;

            this.Controls.Add(new Label { Text = "Prefijo de folio:", Font = Theme.FontSubtitle, Location = new Point(20, y), AutoSize = true });
            y += 28;
            txtPrefix = new TextBox { Location = new Point(20, y), Width = 200, Font = new Font("Segoe UI", 13), Text = "MOV" };
            this.Controls.Add(txtPrefix);

            var lblHint = new Label
            {
                Text = "El folio quedará: MOV-000001, TAB-000001, etc.",
                Font = new Font("Segoe UI", 9), ForeColor = Color.Gray,
                Location = new Point(230, y + 5), AutoSize = true
            };
            this.Controls.Add(lblHint);
            y += 50;

            lblToken = new Label
            {
                Text = "🔐 El token se generará automáticamente al guardar.",
                Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray,
                Location = new Point(20, y), AutoSize = true
            };
            this.Controls.Add(lblToken);
            y += 40;

            btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(20, y), Width = 150, Height = 38 };
            btnCancelar = new Button { Text = "Cancelar", Location = new Point(180, y), Width = 120, Height = 38 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Color.White, Theme.FontNormal);
            Theme.StyleButton(btnCancelar, Color.Gray, Color.White, Theme.FontNormal);
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string prefix = txtPrefix.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(nombre)) { CustomMessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrEmpty(prefix)) prefix = "MOV";

            try
            {
                byte[] tokenBytes = new byte[32];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(tokenBytes);
                }
                string token = Convert.ToBase64String(tokenBytes);
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    int id;
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO Dispositivos(Nombre,Token,PrefixFolio) VALUES(@n,@t,@p) RETURNING Id", conn))
                    {
                        cmd.Parameters.AddWithValue("n", nombre);
                        cmd.Parameters.AddWithValue("t", token);
                        cmd.Parameters.AddWithValue("p", prefix);
                        id = (int)cmd.ExecuteScalar();
                    }
                    using (var cmd2 = new NpgsqlCommand(
                        "INSERT INTO DispositivoFolios(DispositivoId,TipoDocumento,Consecutivo) VALUES(@d,'VENTA',0)", conn))
                    {
                        cmd2.Parameters.AddWithValue("d", id);
                        cmd2.ExecuteNonQuery();
                    }
                }
                CustomMessageBox.Show(
                    $"✅ Dispositivo creado correctamente.\n\n🔐 Token (cópialo para la app Flutter):\n{token}",
                    "Dispositivo Creado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
