using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;

namespace ConexionSQL
{
    public class MigradorSQL(string connectionString)
    {
        public async Task MigrarDesdeArchivos(string archivoPath)
        {
            try
            {
                //  Usar la cadena de conexión proporcionada (sin base de datos especificada)
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master" // Sin base de datos inicial
                };

                using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                // CREAR BASE SI NO EXISTE
                string crearDB = @"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ConexionSQL')
                BEGIN
                    CREATE DATABASE ConexionSQL;
                END";

                using (var cmd = new SqlCommand(crearDB, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                //  CAMBIAR A LA BASE
                connection.ChangeDatabase("ConexionSQL");

                //  CREAR TABLA SI NO EXISTE
                string crearTabla = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Ciudadano' AND xtype='U')
                CREATE TABLE Ciudadano (
                    id INT PRIMARY KEY,
                    Nombre NVARCHAR(50),
                    Edad INT
                )";

                using (var cmd = new SqlCommand(crearTabla, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                //  INSERTAR DATOS DESDE EL ARCHIVO
                var gestor = new GestorDeArchivos(archivoPath);
                int pos = 0;

                while (true)
                {
                    var ciudadano = gestor.LeerCiudadano(pos);

                    if (ciudadano == null)
                        break;

                    string insert = @"
IF NOT EXISTS (SELECT 1 FROM Ciudadano WHERE id = @id)
BEGIN
    INSERT INTO Ciudadano (id, Nombre, Edad)
    VALUES (@id, @Nombre, @Edad)
END";
                    using var insertCmd = new SqlCommand(insert, connection);
                    insertCmd.Parameters.AddWithValue("@id", ciudadano.Value.id);
                    insertCmd.Parameters.AddWithValue("@Nombre", ciudadano.Value.Nombre);
                    insertCmd.Parameters.AddWithValue("@Edad", ciudadano.Value.Edad);

                    await insertCmd.ExecuteNonQueryAsync();

                    pos++;
                }

            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de SQL: {ex.Message}\n\nVerifica:\n- Que SQL Server esté en ejecución\n- Que TCP/IP esté habilitado\n- Que el firewall permita conexiones", "Error de Conexión");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        // NUEVO: Sincronizar un único ciudadano nuevo a SQL
        public async Task SincronizarCiudadano(Ciudadano ciudadano)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "ConexionSQL"
                };

                using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                string insert = @"
IF NOT EXISTS (SELECT 1 FROM Ciudadano WHERE id = @id)
BEGIN
    INSERT INTO Ciudadano (id, Nombre, Edad)
    VALUES (@id, @Nombre, @Edad)
END";

                using var cmd = new SqlCommand(insert, connection);
                cmd.Parameters.AddWithValue("@id", ciudadano.id);
                cmd.Parameters.AddWithValue("@Nombre", ciudadano.Nombre ?? "");
                cmd.Parameters.AddWithValue("@Edad", ciudadano.Edad);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al sincronizar a SQL: {ex.Message}", "Error de Sincronización");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        //  NUEVO: Método para crear la tabla en SQL
        public async Task CrearTablaEnSQL()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };

                using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                // 1️⃣ Crear base de datos
                string crearDB = @"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ConexionSQL')
                BEGIN
                    CREATE DATABASE ConexionSQL;
                END";

                using (var cmd = new SqlCommand(crearDB, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                //  Cambiar a la base
                connection.ChangeDatabase("ConexionSQL");

                // Crear tabla
                string crearTabla = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Ciudadano' AND xtype='U')
                CREATE TABLE Ciudadano (
                    id INT PRIMARY KEY,
                    Nombre NVARCHAR(50),
                    Edad INT
                )";

                using (var cmd = new SqlCommand(crearTabla, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de SQL: {ex.Message}", "Error de Conexión");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    }
}