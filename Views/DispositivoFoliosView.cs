using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using momospos.Helpers;

namespace momospos.Views
{
    public class DispositivoFoliosView : UserControl
    {
        private DataGridView dgv;
        private ComboBox cbDispositivo;
        private Button btnRefrescar, btnResetear;
        private Label lblInfo;

        public DispositivoFoliosView()
        {
            BuildUI();
            CargarDispositivos();
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
                Text = "🔢 Folios por Dispositivo",
                Font = new Font("Segoe UI", small ? 18 : 22, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Margin = new Padding(0, 0, 20, 0)
            };
            topPanel.Controls.Add(lblTitulo);

            var lblFiltro = new Label
            {
                Text = "Dispositivo:", Font = Theme.FontNormal,
                AutoSize = true, Margin = new Padding(5, 10, 5, 0)
            };
            topPanel.Controls.Add(lblFiltro);

            cbDispositivo = new ComboBox
            {
                Width = 220, Height = 36, Font = Theme.FontNormal,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 6, 10, 0)
            };
            cbDispositivo.SelectedIndexChanged += (s, e) => CargarFolios();
            topPanel.Controls.Add(cbDispositivo);

            int bw = small ? 130 : 150, bh = small ? 32 : 36;

            btnRefrescar = new Button { Text = "🔄 Actualizar", Width = bw, Height = bh, Margin = new Padding(5) };
            btnResetear  = new Button { Text = "⚠️ Resetear", Width = bw, Height = bh, Margin = new Padding(5) };
            Theme.StyleButton(btnRefrescar, Theme.SecondaryColor, Color.White, Theme.FontNormal);
            Theme.StyleButton(btnResetear, Theme.DangerColor, Color.White, Theme.FontNormal);
            btnRefrescar.Click += (s, e) => CargarFolios();
            btnResetear.Click  += BtnResetear_Click;
            topPanel.Controls.Add(btnRefrescar);
            topPanel.Controls.Add(btnResetear);

            lblInfo = new Label
            {
                Font = new Font("Segoe UI", 10), ForeColor = Color.Gray,
                AutoSize = true, Margin = new Padding(10, 10, 0, 0)
            };
            topPanel.Controls.Add(lblInfo);

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

        private void CargarDispositivos()
        {
            cbDispositivo.Items.Clear();
            cbDispositivo.Items.Add("(Todos los dispositivos)");
            try
            {
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT Id, Nombre FROM Dispositivos ORDER BY Nombre", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            cbDispositivo.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
                        }
                    }
                }
            }
            catch { }
            cbDispositivo.SelectedIndex = 0;
            CargarFolios();
        }

        private void CargarFolios()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "FolioId",       HeaderText = "ID",            FillWeight = 5 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dispositivo",   HeaderText = "Dispositivo",   FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TipoDoc",       HeaderText = "Tipo Doc.",     FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Consecutivo",  HeaderText = "Consecutivo",   FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "UltimoFolio",  HeaderText = "Último Folio",  FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado",       HeaderText = "Estado",        FillWeight = 10 });

            try
            {
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                string filtro = "";
                int dispId = 0;
                if (cbDispositivo.SelectedItem is ComboItem ci)
                {
                    dispId = ci.Id;
                    filtro = " AND df.DispositivoId = @did";
                }

                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    string sql = $@"
                        SELECT df.Id, d.Nombre, d.PrefixFolio, df.TipoDocumento, df.Consecutivo, df.Activo
                        FROM DispositivoFolios df
                        JOIN Dispositivos d ON d.Id = df.DispositivoId
                        WHERE 1=1{filtro}
                        ORDER BY d.Nombre, df.TipoDocumento";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        if (dispId > 0) cmd.Parameters.AddWithValue("did", dispId);
                        using (var r = cmd.ExecuteReader())
                        {
                            int count = 0;
                            while (r.Read())
                            {
                                count++;
                                long consecutivo = r.GetInt64(4);
                                string prefix    = r.GetString(2);
                                string tipo      = r.GetString(3);
                                bool activo      = r.GetBoolean(5);
                                string ultimoFolio = consecutivo > 0 ? $"{prefix}-{consecutivo:D6}" : "(sin folios aún)";

                                int idx = dgv.Rows.Add(
                                    r.GetInt32(0),
                                    r.GetString(1),
                                    tipo,
                                    consecutivo,
                                    ultimoFolio,
                                    activo ? "✅ Activo" : "🚫 Inactivo"
                                );
                                dgv.Rows[idx].Tag = r.GetInt32(0); // FolioId
                            }
                            lblInfo.Text = $"{count} registro(s) de folio";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al cargar folios:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResetear_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show("Seleccione un registro de folio para resetear.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int folioId = (int)dgv.SelectedRows[0].Tag;

            var res = CustomMessageBox.Show(
                "⚠️ ¿Resetear el consecutivo de este folio a 0?\n\nEsto puede generar folios duplicados si ya hay ventas registradas con este dispositivo.",
                "Confirmar Reseteo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            try
            {
                string cs = ConfiguracionHelper.ObtenerCadenaConexion();
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE DispositivoFolios SET Consecutivo=0 WHERE Id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", folioId);
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarFolios();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Helper para ComboBox con Id
    internal class ComboItem
    {
        public int    Id   { get; }
        public string Name { get; }
        public ComboItem(int id, string name) { Id = id; Name = name; }
        public override string ToString() => Name;
    }
}
