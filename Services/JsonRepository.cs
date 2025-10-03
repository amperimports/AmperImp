using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmperImp.Services
{
    public class JsonRepository<T> where T : class
    {
        private readonly string _rutaArchivo;
        private readonly List<T> _datos;
        private readonly object _fileLock = new object(); // bloqueo para accesos concurrentes
        private const int MaxIntentos = 5;
        private const int DelayMs = 100;

        public JsonRepository(string rutaArchivo)
        {
            _rutaArchivo = rutaArchivo;
            _datos = CargarDatos();
        }

        private List<T> CargarDatos()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<T>();

            lock (_fileLock)
            {
                using (var stream = new FileStream(_rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var opciones = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    return JsonSerializer.Deserialize<List<T>>(json, opciones) ?? new List<T>();
                }
            }
        }

        private void GuardarDatos()
        {
            var opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = JsonSerializer.Serialize(_datos, opciones);

            int intentos = 0;
            while (true)
            {
                try
                {
                    lock (_fileLock)
                    {
                        using (var stream = new FileStream(_rutaArchivo, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(json);
                        }
                    }
                    break; // éxito, salir del loop
                }
                catch (IOException)
                {
                    intentos++;
                    if (intentos > MaxIntentos)
                        throw;
                    Thread.Sleep(DelayMs); // esperar antes de reintentar
                }
            }
        }

        public List<T> GetAll()
        {
            lock (_fileLock)
            {
                return _datos.ToList(); // devolvemos copia para evitar modificaciones externas
            }
        }

        public T? GetById(Func<T, bool> predicate)
        {
            lock (_fileLock)
            {
                return _datos.FirstOrDefault(predicate);
            }
        }

        public void Add(T entidad)
        {
            lock (_fileLock)
            {
                _datos.Add(entidad);
                GuardarDatos();
            }
        }

        public void Update()
        {
            lock (_fileLock)
            {
                GuardarDatos();
            }
        }

        public void UpdateItem(T item, Predicate<T> predicate)
        {
            lock (_fileLock)
            {
                var index = _datos.FindIndex(predicate);
                if (index != -1)
                {
                    _datos[index] = item;
                    GuardarDatos();
                }
            }
        }

        public void Delete(Func<T, bool> predicate)
        {
            lock (_fileLock)
            {
                var entidad = _datos.FirstOrDefault(predicate);
                if (entidad != null)
                {
                    _datos.Remove(entidad);
                    GuardarDatos();
                }
            }
        }
    }
}
