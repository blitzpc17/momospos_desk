using System;
using System.Drawing;
using System.Windows.Forms;
using momospos.Models;
using momospos.Views;

namespace momospos.Views.Dialogs
{
    public class VentaDetalleForm : Form
    {
        private Venta _venta;
        private DataGridView dgvDetalles;
        private Button btnReimprimir;

        public VentaDetalleForm(Venta venta)
        {
            _venta = venta;
            BuildUI();
            CargarDetalles();
        }

        private void BuildUI()
        {
            this.Text = "Detalles de la Venta " + _venta.Folio;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BackgroundColor;

            // CABECERA
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(20) };
            
            Label lblFolio = new Label { Text = $"Folio: {_venta.Folio}", Font = Theme.FontTitle, ForeColor = Theme.PrimaryColor, AutoSize = true, Location = new Point(20, 20) };
            Label lblFecha = new Label { Text = $"Fecha: {_venta.Fecha:dd/MM/yyyy HH:mm:ss}", Font = Theme.FontNormal, AutoSize = true, Location = new Point(20, 60) };
            Label lblEstado = new Label { Text = $"Estado: {_venta.Estado}", Font = Theme.FontNormalBold, ForeColor = (_venta.Estado == "CONFIRMADO" ? Theme.SuccessColor : Theme.DangerColor), AutoSize = true, Location = new Point(20, 85) };

            Label lblTotal = new Label { Text = $"Total: {_venta.Total:C}", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Theme.TextDark, AutoSize = true, Location = new Point(500, 20) };
            Label lblPagado = new Label { Text = $"Pagado: {_venta.Pagado:C}", Font = Theme.FontNormal, AutoSize = true, Location = new Point(500, 60) };
            Label lblCambio = new Label { Text = $"Cambio: {_venta.Cambio:C}", Font = Theme.FontNormal, AutoSize = true, Location = new Point(500, 85) };

            pnlHeader.Controls.Add(lblFolio);
            pnlHeader.Controls.Add(lblFecha);
            pnlHeader.Controls.Add(lblEstado);
            pnlHeader.Controls.Add(lblTotal);
            pnlHeader.Controls.Add(lblPagado);
            pnlHeader.Controls.Add(lblCambio);

            if (!string.IsNullOrEmpty(_venta.MedicoNombre))
            {
                Label lblMedico = new Label { Text = $"Médico: {_venta.MedicoNombre} (Cédula: {_venta.MedicoCedula})", Font = Theme.FontNormal, ForeColor = Color.Teal, AutoSize = true, Location = new Point(20, 110) };
                pnlHeader.Controls.Add(lblMedico);
                pnlHeader.Height = 150;
            }

            // DATAGRIDVIEW
            dgvDetalles = new DataGridView { Dock = DockStyle.Fill };
            Theme.StyleDataGridView(dgvDetalles);
            
            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGrid.Controls.Add(dgvDetalles);

            // PIE / ACCIONES
            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(20) };
            
            btnReimprimir = new Button { Text = "🖨️ Reimprimir Ticket", Width = 200, Height = 40, Location = new Point(20, 15) };
            Theme.StyleButton(btnReimprimir, Theme.PrimaryColor);
            btnReimprimir.Click += BtnReimprimir_Click;

            Button btnGenerarPdf = new Button { Text = "📄 Generar PDF", Width = 150, Height = 40, Location = new Point(230, 15) };
            Theme.StyleButton(btnGenerarPdf, Color.DarkOrange);
            btnGenerarPdf.Click += BtnGenerarPdf_Click;

            Button btnCerrar = new Button { Text = "Cerrar", Width = 100, Height = 40, Location = new Point(650, 15), Anchor = AnchorStyles.Right | AnchorStyles.Top };
            Theme.StyleButton(btnCerrar, Theme.SecondaryColor);
            btnCerrar.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(btnReimprimir);
            pnlFooter.Controls.Add(btnGenerarPdf);
            pnlFooter.Controls.Add(btnCerrar);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private void CargarDetalles()
        {
            dgvDetalles.DataSource = null;
            dgvDetalles.DataSource = _venta.Detalles;

            if (dgvDetalles.Columns["Id"] != null) dgvDetalles.Columns["Id"].Visible = false;
            if (dgvDetalles.Columns["VentaId"] != null) dgvDetalles.Columns["VentaId"].Visible = false;
            if (dgvDetalles.Columns["ProductoId"] != null) dgvDetalles.Columns["ProductoId"].Visible = false;
            
            if (dgvDetalles.Columns["Descripcion"] != null) dgvDetalles.Columns["Descripcion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            if (dgvDetalles.Columns["PrecioUnitario"] != null) dgvDetalles.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            if (dgvDetalles.Columns["Subtotal"] != null) dgvDetalles.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }

        private void BtnReimprimir_Click(object sender, EventArgs e)
        {
            try
            {
                var printer = new TicketPrinter(_venta);
                printer.Imprimir();
                MessageBox.Show("Se envió la orden de impresión exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar reimprimir: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerarPdf_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Documents|*.pdf", FileName = $"Ticket_{_venta.Folio}.pdf" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var printer = new TicketPrinter(_venta);
                        printer.ImprimirComoPdf(sfd.FileName);
                        MessageBox.Show("Se generó el PDF exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al generar PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
