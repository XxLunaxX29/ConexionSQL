using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Text;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConexionSQL
{
    public partial class Form1 : Form
    {
        private GestorDeArchivos gestor;
        private IndexadorArchivos indexador;
        private Dictionary<int, int> indice = new Dictionary<int, int>();
        private int posicionActual = 0;
        private const int REGISTROS_POR_PAGINA = 100;

        string connectionString = "Server=192.168.0.43,1433;User Id=sa;Password=123;TrustServerCertificate=True;Encrypt=False;";

        private MigradorSQL migrador;

        public Form1()
        {
            InitializeComponent();
            ConfigurarGrid();
            ConfigurarControles();

            _ = CargarDatosAsync();

            migrador = new MigradorSQL("Server=192.168.0.43,1433;User Id=sa;Password=123;TrustServerCertificate=True;Encrypt=False;");
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                gestor = await Task.Run(() => new GestorDeArchivos("datos_ciudadanos.dat"));
                indexador = new IndexadorArchivos("datos_ciudadanos.dat");

                posicionActual = gestor.ContarRegistros();

                await migrador.CrearTablaEnSQL();

                CargarPrimeraPagina();

                _ = Task.Run(() => GenerarIndiceEnBackground());

                MostrarResultado($"? Datos cargados correctamente\n({posicionActual:N0} registros)", "success");
            }
            catch (Exception ex)
            {
                MostrarResultado($"Error al cargar datos: {ex.Message}", "error");
            }
        }

        private void GenerarIndiceEnBackground()
        {
            if (gestor == null) return;

            var sw = Stopwatch.StartNew();

            indexador.GenerarIndice(gestor);

            sw.Stop();
            Console.WriteLine($"Índice generado en {sw.ElapsedMilliseconds} ms");

            indice.Clear();
            int pos = 0;

            while (true)
            {
                var ciudadano = gestor.LeerCiudadano(pos);
                if (ciudadano == null) break;

                indice[ciudadano.Value.id] = pos;
                pos++;
            }
        }

        private void ConfigurarControles()
        {
            txtId.ReadOnly = true;
            txtId.BackColor = System.Drawing.SystemColors.Control;
        }

        private void ConfigurarGrid()
        {
            dgvCiudadanos.ColumnCount = 3;

            dgvCiudadanos.Columns[0].Name = "ID";
            dgvCiudadanos.Columns[1].Name = "Nombre";
            dgvCiudadanos.Columns[2].Name = "Edad";

            dgvCiudadanos.AllowUserToAddRows = false;
        }

        private void CargarPrimeraPagina()
        {
            if (gestor == null) return;

            dgvCiudadanos.Rows.Clear();

            int pos = 0;
            int contador = 0;

            while (contador < REGISTROS_POR_PAGINA)
            {
                var ciudadano = gestor.LeerCiudadano(pos);

                if (ciudadano == null)
                    break;

                dgvCiudadanos.Rows.Add(
                    ciudadano.Value.id,
                    ciudadano.Value.Nombre,
                    ciudadano.Value.Edad
                );

                pos++;
                contador++;
            }
        }

        private void AgregarAlGrid(Ciudadano c)
        {
            dgvCiudadanos.Rows.Add(c.id, c.Nombre, c.Edad);
        }

        //  MÉTODO para mostrar resultados
        private void MostrarResultado(string mensaje, string tipo)
        {
            if (InvokeRequired)
            {
                Invoke(() => MostrarResultado(mensaje, tipo));
                return;
            }

            txtResultados.Text = mensaje;

            if (tipo == "success")
                txtResultados.BackColor = System.Drawing.Color.LightGreen;
            else if (tipo == "error")
                txtResultados.BackColor = System.Drawing.Color.LightCoral;
            else
                txtResultados.BackColor = System.Drawing.Color.WhiteSmoke;
        }

        //  MÉTODO para mostrar comparativa (versión simple)
        private void MostrarComparativa(long tiempoL1, long tiempoL2, int posicion, Ciudadano ciudadano)
        {
            if (InvokeRequired)
            {
                Invoke(() => MostrarComparativa(tiempoL1, tiempoL2, posicion, ciudadano));
                return;
            }

            //  Calcular offset: posición × tamaño de registro (58 bytes)
            long offsetBytes = (long)posicion * 58;

            var sb = new StringBuilder();
            sb.AppendLine("==== COMPARATIVA ====");
            sb.AppendLine("");
            sb.AppendLine("[NIVEL 1] Memoria (Dictionary):");
            sb.AppendLine($"  Tiempo: {tiempoL1} ms");
            sb.AppendLine("");
            sb.AppendLine("[NIVEL 2] Archivo (.idx):");
            sb.AppendLine($"  Tiempo: {tiempoL2} ms");
            sb.AppendLine("");
            sb.AppendLine("--- ACCESO AL REGISTRO ---");
            sb.AppendLine($"  Posición: {posicion}");
            sb.AppendLine($"  Offset:   {offsetBytes} bytes");
            sb.AppendLine($"  Fórmula:  {posicion} × 58 = {offsetBytes}");
            sb.AppendLine("");

            if (tiempoL1 < tiempoL2)
                sb.AppendLine($"L1 es {tiempoL2 - tiempoL1} ms más rápido");
            else if (tiempoL2 < tiempoL1)
                sb.AppendLine($"L2 es {tiempoL1 - tiempoL2} ms más rápido");
            else
                sb.AppendLine("Velocidades iguales");

            sb.AppendLine("");
            sb.AppendLine("--- DATOS ---");
            sb.AppendLine($"ID:     {ciudadano.id}");
            sb.AppendLine($"Nombre: {ciudadano.Nombre}");
            sb.AppendLine($"Edad:   {ciudadano.Edad}");

            lblComparativa.Text = sb.ToString();
            lblComparativa.BackColor = System.Drawing.Color.LightCyan;
            lblComparativa.Font = new System.Drawing.Font("Courier New", 8F);
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (gestor == null)
                {
                    MostrarResultado("Los datos aún se están cargando. Por favor espere.", "error");
                    return;
                }

                string nombre = txtNombre.Text;
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MostrarResultado("El nombre no puede estar vacío", "error");
                    return;
                }

                if (!int.TryParse(txtEdad.Text, out int edad))
                {
                    MostrarResultado("Edad inválida", "error");
                    return;
                }

                int nuevoId = (indice.Count > 0 ? indice.Keys.Max() : posicionActual) + 1;

                Ciudadano ciudadano = new Ciudadano
                {
                    id = nuevoId,
                    Nombre = nombre,
                    Edad = edad
                };

                gestor.GuardarCiudadano(ciudadano, posicionActual);
                indice[nuevoId] = posicionActual;
                indexador.AgregarRegistroIndice(nuevoId, posicionActual);

                posicionActual++;
                AgregarAlGrid(ciudadano);

                await migrador.SincronizarCiudadano(ciudadano);

                MostrarResultado($"Ciudadano guardado (ID: {nuevoId})\n? Sincronizado a SQL", "success");
                Limpiar();
                txtId.Text = (nuevoId + 1).ToString();
            }
            catch (Exception ex)
            {
                MostrarResultado($"Error: {ex.Message}", "error");
            }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                if (gestor == null)
                {
                    MostrarResultado("Los datos aún se están cargando.", "error");
                    return;
                }

                if (!int.TryParse(txtBuscarId.Text, out int id))
                {
                    MostrarResultado("ID inválido", "error");
                    return;
                }

                var sw = Stopwatch.StartNew();

                //   Buscar en archivo de índice
                int? posicionL2 = indexador.BuscarPosicion(id);
                sw.Stop();
                long tiempoL2 = sw.ElapsedMilliseconds;

                sw.Restart();

                //    Buscar en diccionario en memoria
                bool existeL1 = indice.ContainsKey(id);
                int posicionL1 = existeL1 ? indice[id] : -1;
                sw.Stop();
                long tiempoL1 = sw.ElapsedMilliseconds;

                if (posicionL2.HasValue && posicionL2.Value >= 0)
                {
                    var ciudadano = gestor.LeerCiudadano(posicionL2.Value);

                    if (ciudadano != null)
                    {
                        txtId.Text = ciudadano.Value.id.ToString();
                        txtNombre.Text = ciudadano.Value.Nombre;
                        txtEdad.Text = ciudadano.Value.Edad.ToString();

                        // ?? Mostrar en el panel
                        MostrarComparativa(tiempoL1, tiempoL2, posicionL2.Value, ciudadano.Value);
                        MostrarResultado($"? Ciudadano encontrado\nID: {ciudadano.Value.id}\nNombre: {ciudadano.Value.Nombre}", "success");
                    }
                }
                else
                {
                    MostrarResultado("No encontrado", "error");
                }
            }
            catch (Exception ex)
            {
                MostrarResultado($"Error: {ex.Message}", "error");
            }
        }

        private async void btnMigrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (gestor == null)
                {
                    MostrarResultado("Los datos aún se están cargando.", "error");
                    return;
                }

                var migrador = new MigradorSQL(connectionString);

                MostrarResultado("? Migrando datos a SQL Server...", "info");
                await migrador.MigrarDesdeArchivos("datos_ciudadanos.dat");

                MostrarResultado("? Datos migrados correctamente a SQL Server", "success");
            }
            catch (Exception ex)
            {
                MostrarResultado($"Error al migrar: {ex.Message}", "error");
            }
        }

        private async void btnProbarConexion_Click(object sender, EventArgs e)
        {
            var diagnostico = new StringBuilder();
            diagnostico.AppendLine("=== DIAGNÓSTICO DE CONEXIÓN ===\n");

            try
            {
                diagnostico.AppendLine("? 1. Ping a 192.168.0.43");
                diagnostico.AppendLine("\n? 2. Probando conexión a SQL Server...");
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                diagnostico.AppendLine("? Conexión exitosa");

                using var cmd = new SqlCommand("SELECT @@SERVERNAME AS Servidor, @@VERSION AS Version", connection);
                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    diagnostico.AppendLine($"\n? Servidor: {reader["Servidor"]}");
                    diagnostico.AppendLine($"? Versión: {reader["Version"]}");
                }

                MostrarResultado(diagnostico.ToString(), "success");
            }
            catch (SqlException ex) when (ex.Number == -1)
            {
                diagnostico.AppendLine("? ERROR: Timeout o instancia no encontrada");
                MostrarResultado(diagnostico.ToString(), "error");
            }
            catch (SqlException ex) when (ex.Number == 18456)
            {
                diagnostico.AppendLine("? ERROR: Credenciales incorrectas");
                MostrarResultado(diagnostico.ToString(), "error");
            }
            catch (Exception ex)
            {
                diagnostico.AppendLine($"? ERROR: {ex.Message}");
                MostrarResultado(diagnostico.ToString(), "error");
            }
        }

        private void Limpiar()
        {
            txtId.Clear();
            txtNombre.Clear();
            txtEdad.Clear();
        }


        private void btnBuscar2_Click(object sender, EventArgs e)
        {
            try
            {
                if (gestor == null)
                {
                    MostrarResultado("Los datos aún se están cargando.", "error");
                    return;
                }

                if (!int.TryParse(txtBuscarId.Text, out int id))
                {
                    MostrarResultado("ID inválido", "error");
                    return;
                }

                //   Búsqueda SECUENCIAL
                var resultadoL1 = gestor.BuscarSecuencial(id);

                //   Búsqueda INDEXADA
                var resultadoL2 = indexador.BuscarPosicionConMetricas(id);

                if (resultadoL1.Ciudadano.HasValue && resultadoL1.Posicion >= 0)
                {
                    var ciudadano = resultadoL1.Ciudadano.Value;
                    txtId.Text = ciudadano.id.ToString();
                    txtNombre.Text = ciudadano.Nombre;
                    txtEdad.Text = ciudadano.Edad.ToString();

                    // /Mostrar comparativa CON BÚSQUEDAS
                    MostrarComparativaBusquedas(
                        resultadoL1.TiempoMs,
                        resultadoL1.RegistrosRecorridos,
                        resultadoL2.TiempoMs,
                        resultadoL2.BytesLeidos,
                        resultadoL1.Posicion,
                        ciudadano
                    );

                    MostrarResultado($"? Ciudadano encontrado\nID: {ciudadano.id}\nNombre: {ciudadano.Nombre}", "success");
                }
                else
                {
                    MostrarResultado("No encontrado", "error");
                }
            }
            catch (Exception ex)
            {
                MostrarResultado($"Error: {ex.Message}", "error");
            }
        }

        // MÉTODO para mostrar comparativa de BÚSQUEDAS
        private void MostrarComparativaBusquedas(long tiempoL1, long registrosL1, long tiempoL2, long bytesL2, int posicion, Ciudadano ciudadano)
        {
            if (InvokeRequired)
            {
                Invoke(() => MostrarComparativaBusquedas(tiempoL1, registrosL1, tiempoL2, bytesL2, posicion, ciudadano));
                return;
            }

            //  Calcular offset: posición × tamaño de registro (58 bytes)
            long offsetBytes = (long)posicion * 58;

            var sb = new StringBuilder();
            sb.AppendLine("==== BÚSQUEDA: SECUENCIAL vs INDEXADA ====");
            sb.AppendLine("");
            sb.AppendLine("[ NIVEL 1 ] BÚSQUEDA SECUENCIAL");
            sb.AppendLine($"  Tiempo:               {tiempoL1} ms");
            sb.AppendLine($"  Registros recorridos: {registrosL1}");
            sb.AppendLine($"  Método: Leer uno a uno hasta encontrar");
            sb.AppendLine("");
            sb.AppendLine("[ NIVEL 2 ] BÚSQUEDA INDEXADA");
            sb.AppendLine($"  Tiempo:       {tiempoL2} ms");
            sb.AppendLine($"  Bytes leídos: {bytesL2}");
            sb.AppendLine($"  Método: Consultar índice (.idx)");
            sb.AppendLine("");
            sb.AppendLine("--- ACCESO AL REGISTRO ---");
            sb.AppendLine($"  Posición:     {posicion}");
            sb.AppendLine($"  Offset:       {offsetBytes} bytes");
            sb.AppendLine($"  Tamaño:       58 bytes (4 + 50 + 4)");
            sb.AppendLine($"  Cálculo:      {posicion} × 58 = {offsetBytes}");
            sb.AppendLine("");
            sb.AppendLine("--- COMPARACIÓN ---");

            long diferencia = Math.Abs(tiempoL1 - tiempoL2);
            double porcentaje = (tiempoL1 > 0) ? ((double)diferencia / tiempoL1) * 100 : 0;

            if (tiempoL1 > tiempoL2)
            {
                sb.AppendLine($"GANADOR: L2 (indexada)");
                sb.AppendLine($"  Ventaja: {diferencia} ms más rápido ({porcentaje:F1}%)");
                sb.AppendLine($"  Razón: Acceso directo sin leer {registrosL1} registros");
            }
            else if (tiempoL2 > tiempoL1)
            {
                sb.AppendLine($"GANADOR: L1 (secuencial)");
                sb.AppendLine($"  Ventaja: {diferencia} ms más rápido ({porcentaje:F1}%)");
            }
            else
            {
                sb.AppendLine("Velocidades similares");
            }

            sb.AppendLine("");
            sb.AppendLine("--- DATOS ENCONTRADOS ---");
            sb.AppendLine($"  ID:     {ciudadano.id}");
            sb.AppendLine($"  Nombre: {ciudadano.Nombre?.Substring(0, Math.Min(30, ciudadano.Nombre?.Length ?? 0))}");
            sb.AppendLine($"  Edad:   {ciudadano.Edad}");

            lblComparativa.Text = sb.ToString();
            lblComparativa.BackColor = System.Drawing.Color.LightYellow;
            lblComparativa.Font = new System.Drawing.Font("Courier New", 8F);
        }
    }
}
