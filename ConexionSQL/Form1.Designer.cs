namespace ConexionSQL
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtEdad;
        private System.Windows.Forms.TextBox txtBuscarId;

        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnMigrar;
        private System.Windows.Forms.Button btnProbarConexion;

        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblEdad;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.Label lblResultados;
        private System.Windows.Forms.Label lblComparativa;

        private System.Windows.Forms.TextBox txtResultados;
        private System.Windows.Forms.Panel panelComparativa;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtId = new TextBox();
            txtNombre = new TextBox();
            txtEdad = new TextBox();
            txtBuscarId = new TextBox();
            btnGuardar = new Button();
            btnBuscar = new Button();
            btnMigrar = new Button();
            btnProbarConexion = new Button();
            lblId = new Label();
            lblNombre = new Label();
            lblEdad = new Label();
            lblBuscar = new Label();
            lblResultados = new Label();
            lblComparativa = new Label();
            txtResultados = new TextBox();
            panelComparativa = new Panel();
            dgvCiudadanos = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvCiudadanos).BeginInit();
            SuspendLayout();
            // 
            // txtId
            // 
            txtId.Location = new Point(153, 16);
            txtId.Name = "txtId";
            txtId.Size = new Size(138, 27);
            txtId.TabIndex = 5;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(153, 60);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(138, 27);
            txtNombre.TabIndex = 6;
            // 
            // txtEdad
            // 
            txtEdad.Location = new Point(153, 102);
            txtEdad.Name = "txtEdad";
            txtEdad.Size = new Size(138, 27);
            txtEdad.TabIndex = 7;
            // 
            // txtBuscarId
            // 
            txtBuscarId.Location = new Point(153, 144);
            txtBuscarId.Name = "txtBuscarId";
            txtBuscarId.Size = new Size(138, 27);
            txtBuscarId.TabIndex = 9;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(297, 56);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(132, 34);
            btnGuardar.TabIndex = 10;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(297, 102);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(132, 27);
            btnBuscar.TabIndex = 12;
            btnBuscar.Text = "Buscar";
            btnBuscar.Click += btnBuscar2_Click;
            // 
            // btnMigrar
            // 
            btnMigrar.Location = new Point(297, 12);
            btnMigrar.Name = "btnMigrar";
            btnMigrar.Size = new Size(132, 27);
            btnMigrar.TabIndex = 13;
            btnMigrar.Text = "Migrar a SQL";
            btnMigrar.Click += btnMigrar_Click;
            // 
            // btnProbarConexion
            // 
            btnProbarConexion.Location = new Point(297, 144);
            btnProbarConexion.Name = "btnProbarConexion";
            btnProbarConexion.Size = new Size(132, 27);
            btnProbarConexion.TabIndex = 14;
            btnProbarConexion.Text = "Probar Conexión";
            btnProbarConexion.Click += btnProbarConexion_Click;
            // 
            // lblId
            // 
            lblId.Location = new Point(20, 20);
            lblId.Name = "lblId";
            lblId.Size = new Size(100, 23);
            lblId.TabIndex = 0;
            lblId.Text = "ID:";
            // 
            // lblNombre
            // 
            lblNombre.Location = new Point(20, 60);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(100, 23);
            lblNombre.TabIndex = 1;
            lblNombre.Text = "Nombre:";
            // 
            // lblEdad
            // 
            lblEdad.Location = new Point(20, 100);
            lblEdad.Name = "lblEdad";
            lblEdad.Size = new Size(100, 23);
            lblEdad.TabIndex = 2;
            lblEdad.Text = "Edad:";
            // 
            // lblBuscar
            // 
            lblBuscar.Location = new Point(20, 148);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(100, 23);
            lblBuscar.TabIndex = 4;
            lblBuscar.Text = "Buscar ID:";
            // 
            // lblResultados
            // 
            lblResultados.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResultados.Location = new Point(450, 20);
            lblResultados.Name = "lblResultados";
            lblResultados.Size = new Size(150, 23);
            lblResultados.TabIndex = 16;
            lblResultados.Text = "🔍 Resultados de Búsqueda:";
            // 
            // lblComparativa
            // 
            lblComparativa.BackColor = Color.LightYellow;
            lblComparativa.BorderStyle = BorderStyle.FixedSingle;
            lblComparativa.Font = new Font("Courier New", 8F);
            lblComparativa.Location = new Point(450, 50);
            lblComparativa.Name = "lblComparativa";
            lblComparativa.Size = new Size(200, 516);
            lblComparativa.TabIndex = 17;
            // 
            // txtResultados
            // 
            txtResultados.BackColor = Color.WhiteSmoke;
            txtResultados.Location = new Point(703, 50);
            txtResultados.Multiline = true;
            txtResultados.Name = "txtResultados";
            txtResultados.ReadOnly = true;
            txtResultados.Size = new Size(285, 200);
            txtResultados.TabIndex = 18;
            // 
            // panelComparativa
            // 
            panelComparativa.BackColor = Color.LightYellow;
            panelComparativa.BorderStyle = BorderStyle.FixedSingle;
            panelComparativa.Location = new Point(450, 50);
            panelComparativa.Name = "panelComparativa";
            panelComparativa.Size = new Size(244, 517);
            panelComparativa.TabIndex = 19;
            // 
            // dgvCiudadanos
            // 
            dgvCiudadanos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCiudadanos.Location = new Point(12, 225);
            dgvCiudadanos.Name = "dgvCiudadanos";
            dgvCiudadanos.RowHeadersWidth = 51;
            dgvCiudadanos.Size = new Size(430, 342);
            dgvCiudadanos.TabIndex = 15;
            // 
            // Form1
            // 
            ClientSize = new Size(1000, 580);
            Controls.Add(lblResultados);
            Controls.Add(lblComparativa);
            Controls.Add(txtResultados);
            Controls.Add(panelComparativa);
            Controls.Add(dgvCiudadanos);
            Controls.Add(lblId);
            Controls.Add(lblNombre);
            Controls.Add(lblEdad);
            Controls.Add(lblBuscar);
            Controls.Add(txtId);
            Controls.Add(txtNombre);
            Controls.Add(txtEdad);
            Controls.Add(txtBuscarId);
            Controls.Add(btnGuardar);
            Controls.Add(btnBuscar);
            Controls.Add(btnMigrar);
            Controls.Add(btnProbarConexion);
            Name = "Form1";
            Text = "Gestor de Ciudadanos - Nivel 1 vs Nivel 2";
            ((System.ComponentModel.ISupportInitialize)dgvCiudadanos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private DataGridView dgvCiudadanos;
    }
}
