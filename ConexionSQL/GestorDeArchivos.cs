using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ConexionSQL
{
    //  Estructura con tamaño fijo
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Ciudadano
    {
        public int id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Nombre;

        public int Edad;

        public static int Size => Marshal.SizeOf<Ciudadano>();
    }

    //  ESTRUCTURA para retornar resultados con métricas
    public struct ResultadoLectura
    {
        public Ciudadano? Ciudadano { get; set; }
        public long TiempoOffsetMs { get; set; }
        public long TiempoLecturaMs { get; set; }
        public long TiempoTotalMs { get; set; }
        public long OffsetBytes { get; set; }
    }

    //  ESTRUCTURA para búsqueda secuencial
    public struct ResultadoBusquedaSecuencial
    {
        public Ciudadano? Ciudadano { get; set; }
        public int Posicion { get; set; }
        public long TiempoMs { get; set; }
        public long RegistrosRecorridos { get; set; }
    }

    public class GestorDeArchivos
    {
        private readonly string _path;
        private const int NOMBRE_SIZE = 50;

        public GestorDeArchivos(string path)
        {
            _path = path;

            if (File.Exists("datos_ciudadanos.dat"))
                File.Delete("datos_ciudadanos.dat");

            InicializarDatosPredeterminados();
        }

        //  INICIALIZAR DATOS PREDETERMINADOS
        private void InicializarDatosPredeterminados()
        {
            // Si el archivo ya existe, NO hacer nada (preservar datos)
            if (File.Exists(_path) && new FileInfo(_path).Length > 0)
                return;

            // 10 registros establecidos
            var ciudadanos = new[]
            {
                new Ciudadano { id = 1, Nombre = "Juan García López", Edad = 35 },
                new Ciudadano { id = 2, Nombre = "María Martínez Rodríguez", Edad = 28 },
                new Ciudadano { id = 3, Nombre = "Carlos González Fernández", Edad = 42 },
                new Ciudadano { id = 4, Nombre = "Ana Pérez Sánchez", Edad = 31 },
                new Ciudadano { id = 5, Nombre = "Luis Gómez Jiménez", Edad = 45 },
                new Ciudadano { id = 6, Nombre = "Rosa Díaz Moreno", Edad = 29 },
                new Ciudadano { id = 7, Nombre = "Miguel Navarro Castillo", Edad = 38 },
                new Ciudadano { id = 8, Nombre = "Isabel Ruiz Ramos", Edad = 33 },
                new Ciudadano { id = 9, Nombre = "Diego Herrera Ortiz", Edad = 50 },
                new Ciudadano { id = 10, Nombre = "Carmen Medina Vargas", Edad = 26 }
            };

            // Guardar los 10 ciudadanos
            for (int i = 0; i < ciudadanos.Length; i++)
            {
                GuardarCiudadano(ciudadanos[i], i);
            }

            // Generar registros adicionales (IDs 11 – 5.000)
            for (int n = 10; n < 5_000; n++)
            {
                GuardarCiudadano(new Ciudadano { id = n + 1, Nombre = $"Ciudadano_{n + 1}", Edad = 20 + (n % 60) }, n);
                if ((n + 1) % 1_000 == 0) Console.Write(".");
            }
        }

        //  GUARDAR
        public void GuardarCiudadano(Ciudadano c, int posicion)
        {
            using var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write);

            long offset = (long)posicion * GetRecordSize();
            fs.Seek(offset, SeekOrigin.Begin);

            using var writer = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);

            writer.Write(c.id);
            
            byte[] nombreBytes = Encoding.UTF8.GetBytes(c.Nombre ?? "");
            byte[] nombrePadded = new byte[NOMBRE_SIZE];
            Array.Copy(nombreBytes, nombrePadded, Math.Min(nombreBytes.Length, NOMBRE_SIZE));
            writer.Write(nombrePadded);
            
            writer.Write(c.Edad);
        }

        //  LEER (sin métricas)
        public Ciudadano? LeerCiudadano(int posicion)
        {
            if (!File.Exists(_path)) return null;

            using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read);

            long offset = (long)posicion * GetRecordSize();

            if (offset >= fs.Length) return null;

            fs.Seek(offset, SeekOrigin.Begin);

            using var reader = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

            int id = reader.ReadInt32();
            
            byte[] nombreBytes = reader.ReadBytes(NOMBRE_SIZE);
            string nombre = Encoding.UTF8.GetString(nombreBytes).TrimEnd('\0');
            
            int edad = reader.ReadInt32();

            return new Ciudadano
            {
                id = id,
                Nombre = nombre,
                Edad = edad
            };
        }

        //  LEER CON MÉTRICAS DETALLADAS
        public ResultadoLectura LeerCiudadanoConMetricas(int posicion)
        {
            var resultado = new ResultadoLectura();

            if (!File.Exists(_path))
                return resultado;

            using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read);

            long offset = (long)posicion * GetRecordSize();
            resultado.OffsetBytes = offset;

            if (offset >= fs.Length)
                return resultado;

            //  MEDIR TIEMPO DE OFFSET/SEEK
            var swOffset = Stopwatch.StartNew();
            fs.Seek(offset, SeekOrigin.Begin);
            swOffset.Stop();
            resultado.TiempoOffsetMs = swOffset.ElapsedMilliseconds;

            // MEDIR TIEMPO DE LECTURA
            var swLectura = Stopwatch.StartNew();

            using var reader = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

            int id = reader.ReadInt32();
            byte[] nombreBytes = reader.ReadBytes(NOMBRE_SIZE);
            string nombre = Encoding.UTF8.GetString(nombreBytes).TrimEnd('\0');
            int edad = reader.ReadInt32();

            swLectura.Stop();
            resultado.TiempoLecturaMs = swLectura.ElapsedMilliseconds;
            resultado.TiempoTotalMs = resultado.TiempoOffsetMs + resultado.TiempoLecturaMs;

            resultado.Ciudadano = new Ciudadano
            {
                id = id,
                Nombre = nombre,
                Edad = edad
            };

            return resultado;
        }

        //  BÚSQUEDA SECUENCIAL (NIVEL 1)
        public ResultadoBusquedaSecuencial BuscarSecuencial(int idBuscado)
        {
            var resultado = new ResultadoBusquedaSecuencial { Posicion = -1 };

            if (!File.Exists(_path))
                return resultado;

            var sw = Stopwatch.StartNew();
            int posicion = 0;
            long registrosRecorridos = 0;

            while (true)
            {
                var ciudadano = LeerCiudadano(posicion);

                if (ciudadano == null)
                    break;

                registrosRecorridos++;

                if (ciudadano.Value.id == idBuscado)
                {
                    sw.Stop();
                    resultado.Ciudadano = ciudadano;
                    resultado.Posicion = posicion;
                    resultado.TiempoMs = sw.ElapsedMilliseconds;
                    resultado.RegistrosRecorridos = registrosRecorridos;
                    return resultado;
                }

                posicion++;
            }

            sw.Stop();
            resultado.TiempoMs = sw.ElapsedMilliseconds;
            resultado.RegistrosRecorridos = registrosRecorridos;
            return resultado;
        }

        // OBTENER TAMAÑO DE REGISTRO
        private int GetRecordSize()
        {
            return sizeof(int) + NOMBRE_SIZE + sizeof(int);
        }

        //  CONTAR REGISTROS EN DISCO
        public int ContarRegistros()
        {
            if (!File.Exists(_path)) return 0;
            long fileSize = new FileInfo(_path).Length;
            return (int)(fileSize / GetRecordSize());
        }

        //  REINICIALIZAR
        public void ReinicializarDatos()
        {
            if (File.Exists(_path))
                File.Delete(_path);

            InicializarDatosPredeterminados();
        }
    }
}