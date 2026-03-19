using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ConexionSQL
{
    //  ESTRUCTURA DEL ÍNDICE (tamaño fijo: 8 bytes)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegistroIndice
    {
        public int id;           // 4 bytes
        public int posicion;     // 4 bytes
    }

    // ESTRUCTURA para retornar resultados con métricas
    public struct ResultadoBusquedaIndice
    {
        public int? Posicion { get; set; }
        public long TiempoMs { get; set; }
        public long BytesLeidos { get; set; }
    }

    /// <summary>
    /// NIVEL 2: El Indexador - Archivos Indexados
    /// Crea un archivo secundario (.idx) que mapea ID -> Posición
    /// Permite búsqueda secuencial ultra-rápida sin cargar todo en memoria
    /// </summary>
    public class IndexadorArchivos
    {
        private readonly string _archivoIndice;
        private const int TAMAÑO_REGISTRO_INDICE = sizeof(int) + sizeof(int); // 8 bytes

        public IndexadorArchivos(string archivosDatos)
        {
            // El archivo de índice usa extensión .idx
            _archivoIndice = Path.ChangeExtension(archivosDatos, ".idx");
        }

        //  GENERAR ÍNDICE desde archivo de datos
        public void GenerarIndice(GestorDeArchivos gestor)
        {
            // Eliminar índice anterior si existe
            if (File.Exists(_archivoIndice))
                File.Delete(_archivoIndice);

            using var fs = new FileStream(_archivoIndice, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(fs, Encoding.UTF8);

            int posicion = 0;

            while (true)
            {
                var ciudadano = gestor.LeerCiudadano(posicion);

                if (ciudadano == null)
                    break;

                // Escribir entrada de índice: (id, posicion)
                writer.Write(ciudadano.Value.id);
                writer.Write(posicion);

                posicion++;
            }

            Console.WriteLine($"? Índice generado: {_archivoIndice} ({posicion:N0} registros)");
        }

        //  BUSCAR POR ID (CON MÉTRICAS)
        public ResultadoBusquedaIndice BuscarPosicionConMetricas(int id)
        {
            var resultado = new ResultadoBusquedaIndice();

            if (!File.Exists(_archivoIndice))
                return resultado;

            var sw = Stopwatch.StartNew();

            using var fs = new FileStream(_archivoIndice, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs, Encoding.UTF8);

            while (fs.Position < fs.Length)
            {
                int idLeido = reader.ReadInt32();
                int posicion = reader.ReadInt32();
                resultado.BytesLeidos += TAMAÑO_REGISTRO_INDICE;

                if (idLeido == id)
                {
                    resultado.Posicion = posicion;
                    sw.Stop();
                    resultado.TiempoMs = sw.ElapsedMilliseconds;
                    return resultado;
                }
            }

            sw.Stop();
            resultado.TiempoMs = sw.ElapsedMilliseconds;
            return resultado;
        }

        //  BUSCAR POR ID (sin métricas - original)
        public int? BuscarPosicion(int id)
        {
            if (!File.Exists(_archivoIndice))
                return null;

            using var fs = new FileStream(_archivoIndice, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs, Encoding.UTF8);

            while (fs.Position < fs.Length)
            {
                int idLeido = reader.ReadInt32();
                int posicion = reader.ReadInt32();

                if (idLeido == id)
                    return posicion;
            }

            return null;
        }

        //  AGREGAR ENTRADA AL ÍNDICE
        public void AgregarRegistroIndice(int id, int posicion)
        {
            using var fs = new FileStream(_archivoIndice, FileMode.Append, FileAccess.Write);
            using var writer = new BinaryWriter(fs, Encoding.UTF8);

            writer.Write(id);
            writer.Write(posicion);
        }

        //  CONTAR REGISTROS EN EL ÍNDICE
        public int ContarRegistrosIndice()
        {
            if (!File.Exists(_archivoIndice))
                return 0;

            long fileSize = new FileInfo(_archivoIndice).Length;
            return (int)(fileSize / TAMAÑO_REGISTRO_INDICE);
        }

        //  REINICIALIZAR ÍNDICE
        public void ReinicializarIndice()
        {
            if (File.Exists(_archivoIndice))
                File.Delete(_archivoIndice);
        }
    }
}