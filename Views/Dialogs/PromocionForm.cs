using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using momospos.Models;
using momospos.Repositories;

namespace momospos.Views.Dialogs
{
    public class PromocionForm : Form
    {
        public Promocion PromocionConfigurada { get; private set; }
        private bool _esEdicion;
        
        private TextBox txtNombre;
        private TextBox txtProducto;
        private Button btnBuscarProducto;
        private int? _productoIdSeleccionado = null;
        private ComboBox cbTipo;
        private NumericUpDown nudCantidadRequerida;
        private NumericUpDown nudCantidadRegalo;
        private NumericUpDown nudDescuento;
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;
        private CheckBox chkActivo;
        
        private ProductoRepository _prodRepo = new ProductoRepository();

        public PromocionForm(Promocion promo = null)
        {
            _esEdicion = promo != null;
            PromocionConfigurada = promo ?? new Promocion { 
                FechaInicio = DateTime.Today, 
                FechaFin = DateTime.Today.AddDays(30), 
                Activo = true,
                Tipo = "NxM"
            };
            
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            this.Text = _esEdicion ? "Editar Promoción" : "Nueva Promoción";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.BackgroundColor;

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.PrimaryColor };
            Label lblTitulo = new Label { Text = this.Text, Font = Theme.FontTitle, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15) };
            topPanel.Controls.Add(lblTitulo);
            this.Controls.Add(topPanel);

            int y = 80;
            int spacing = 65;

            // Nombre
            this.Controls.Add(new Label { Text = "Nombre de la promoción (ej. 3x2 Paracetamol):", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            txtNombre = new TextBox { Location = new Point(30, y + 25), Width = 420, Font = new Font("Segoe UI", 12) };
            this.Controls.Add(txtNombre);
            y += spacing;

            // Producto
            this.Controls.Add(new Label { Text = "Producto aplica:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            txtProducto = new TextBox { Location = new Point(30, y + 25), Width = 310, Font = new Font("Segoe UI", 12), ReadOnly = true, BackColor = Color.White };
            this.Controls.Add(txtProducto);
            
            btnBuscarProducto = new Button { Text = "🔍 Buscar", Location = new Point(350, y + 24), Width = 100, Height = 32 };
            Theme.StyleButton(btnBuscarProducto, Theme.SecondaryColor);
            btnBuscarProducto.Click += (s, e) => {
                var buscador = new BuscadorProductoForm();
                if (buscador.ShowDialog() == DialogResult.OK && buscador.ProductoSeleccionado != null)
                {
                    _productoIdSeleccionado = buscador.ProductoSeleccionado.Id;
                    txtProducto.Text = buscador.ProductoSeleccionado.Nombre;
                }
            };
            this.Controls.Add(btnBuscarProducto);
            y += spacing;

            // Tipo
            this.Controls.Add(new Label { Text = "Tipo de Promoción:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            cbTipo = new ComboBox { Location = new Point(30, y + 25), Width = 420, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipo.Items.Add("NxM");
            cbTipo.Items.Add("Porcentaje");
            cbTipo.SelectedIndexChanged += CbTipo_SelectedIndexChanged;
            this.Controls.Add(cbTipo);
            y += spacing;

            // Panel dinámico
            Panel pnlDinamico = new Panel { Location = new Point(30, y), Size = new Size(420, 70) };
            
            // Cantidades para NxM
            Label lblReq = new Label { Text = "Llevas:", Location = new Point(0, 0), AutoSize = true, Font = Theme.FontNormal };
            nudCantidadRequerida = new NumericUpDown { Location = new Point(0, 25), Width = 100, Font = new Font("Segoe UI", 12), DecimalPlaces = 2 };
            
            Label lblReg = new Label { Text = "Pagas:", Location = new Point(120, 0), AutoSize = true, Font = Theme.FontNormal };
            nudCantidadRegalo = new NumericUpDown { Location = new Point(120, 25), Width = 100, Font = new Font("Segoe UI", 12), DecimalPlaces = 2 }; 
            lblReg.Text = "Regalados (Gratis):";
            
            // Porcentaje
            Label lblDesc = new Label { Text = "% Descuento:", Location = new Point(0, 0), AutoSize = true, Font = Theme.FontNormal };
            nudDescuento = new NumericUpDown { Location = new Point(0, 25), Width = 100, Font = new Font("Segoe UI", 12), DecimalPlaces = 2, Maximum = 100 };

            pnlDinamico.Controls.Add(lblReq);
            pnlDinamico.Controls.Add(nudCantidadRequerida);
            pnlDinamico.Controls.Add(lblReg);
            pnlDinamico.Controls.Add(nudCantidadRegalo);
            pnlDinamico.Controls.Add(lblDesc);
            pnlDinamico.Controls.Add(nudDescuento);
            this.Controls.Add(pnlDinamico);
            
            y += spacing + 10;

            // Fechas
            this.Controls.Add(new Label { Text = "Vigencia:", Location = new Point(30, y), AutoSize = true, Font = Theme.FontNormal });
            dtpInicio = new DateTimePicker { Location = new Point(30, y + 25), Width = 150, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            this.Controls.Add(new Label { Text = "al", Location = new Point(190, y + 30), AutoSize = true, Font = Theme.FontNormal });
            dtpFin = new DateTimePicker { Location = new Point(220, y + 25), Width = 150, Font = new Font("Segoe UI", 12), Format = DateTimePickerFormat.Short };
            
            this.Controls.Add(dtpInicio);
            this.Controls.Add(dtpFin);
            
            chkActivo = new CheckBox { Text = "Activo", Location = new Point(390, y + 27), AutoSize = true, Font = Theme.FontNormal };
            this.Controls.Add(chkActivo);
            
            y += spacing;

            Button btnGuardar = new Button { Text = "💾 Guardar", Location = new Point(30, y + 20), Width = 200, Height = 45 };
            Theme.StyleButton(btnGuardar, Theme.PrimaryColor, Theme.TextLight, Theme.FontSubtitle);
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "❌ Cancelar", Location = new Point(250, y + 20), Width = 200, Height = 45 };
            Theme.StyleButton(btnCancelar, Color.Gray, Theme.TextLight, Theme.FontSubtitle);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void CbTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tipo = cbTipo.SelectedItem?.ToString();
            bool esNxM = tipo == "NxM";
            
            nudCantidadRequerida.Visible = esNxM;
            nudCantidadRegalo.Visible = esNxM;
            foreach(Control c in nudCantidadRequerida.Parent.Controls)
            {
                if(c is Label l)
                {
                    if (l.Text == "Llevas:" || l.Text == "Regalados:") l.Visible = esNxM;
                    if (l.Text == "% Descuento:") l.Visible = !esNxM;
                }
            }
            nudDescuento.Visible = !esNxM;
        }

        private void CargarDatos()
        {
            txtNombre.Text = PromocionConfigurada.Nombre;
            cbTipo.SelectedItem = PromocionConfigurada.Tipo;
            
            if(PromocionConfigurada.ProductoId > 0)
            {
                _productoIdSeleccionado = PromocionConfigurada.ProductoId;
                var prod = _prodRepo.ObtenerTodos().FirstOrDefault(p => p.Id == PromocionConfigurada.ProductoId);
                if (prod != null) txtProducto.Text = prod.Nombre;
            }

            nudCantidadRequerida.Value = PromocionConfigurada.CantidadRequerida > 0 ? PromocionConfigurada.CantidadRequerida : 0;
            nudCantidadRegalo.Value = PromocionConfigurada.CantidadRegalo > 0 ? PromocionConfigurada.CantidadRegalo : 0;
            nudDescuento.Value = PromocionConfigurada.DescuentoPorcentaje > 0 ? PromocionConfigurada.DescuentoPorcentaje : 0;
            
            dtpInicio.Value = PromocionConfigurada.FechaInicio > DateTime.MinValue ? PromocionConfigurada.FechaInicio : DateTime.Today;
            dtpFin.Value = PromocionConfigurada.FechaFin > DateTime.MinValue ? PromocionConfigurada.FechaFin : DateTime.Today.AddDays(30);
            
            chkActivo.Checked = PromocionConfigurada.Activo;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                CustomDialog.ShowError("El nombre es requerido.");
                return;
            }
            if (_productoIdSeleccionado == null || _productoIdSeleccionado <= 0)
            {
                CustomDialog.ShowError("Seleccione un producto.");
                return;
            }
            if (cbTipo.SelectedItem == null)
            {
                CustomDialog.ShowError("Seleccione un tipo de promoción.");
                return;
            }

            PromocionConfigurada.Nombre = txtNombre.Text.Trim();
            PromocionConfigurada.ProductoId = _productoIdSeleccionado.Value;
            PromocionConfigurada.Tipo = cbTipo.SelectedItem.ToString();
            PromocionConfigurada.CantidadRequerida = nudCantidadRequerida.Value;
            PromocionConfigurada.CantidadRegalo = nudCantidadRegalo.Value;
            PromocionConfigurada.DescuentoPorcentaje = nudDescuento.Value;
            PromocionConfigurada.FechaInicio = dtpInicio.Value.Date;
            PromocionConfigurada.FechaFin = dtpFin.Value.Date.AddDays(1).AddTicks(-1); // End of day
            PromocionConfigurada.Activo = chkActivo.Checked;

            this.DialogResult = DialogResult.OK;
        }
    }
}
