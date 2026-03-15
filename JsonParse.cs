using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleOntoDoc
{
    /// <summary>
    /// Парсер JSON-формата онтологической схемы, соответствующего выводу model.py.
    /// Принимает JSON-массив объектов Class (см. model.py) и строит граф классов,
    /// разрешая строковые ссылки (sub_class, range) в реальные объекты через GetOrCreate.
    /// </summary>
    internal class JsonParse
    {
        private readonly Dictionary<string, Class> _classes = new();

        public Dictionary<string, Class> Classes => _classes;

        public JsonParse(Program.Options options)
        {
            Log($"Начало парсинга JSON файла: {options.InputJsonPath}");
            Parse(options.InputJsonPath);
            Log("Парсинг завершён.");
        }

        /// <summary>
        /// Возвращает существующий класс по имени или создаёт заглушку, если он ещё не зарегистрирован.
        /// Это позволяет обрабатывать ссылки на классы, которые появятся позже в файле.
        /// </summary>
        private Class GetOrCreate(string name)
        {
            if (!_classes.TryGetValue(name, out var cls))
            {
                cls = new Class { Name = name };
                _classes[name] = cls;
            }
            return cls;
        }

        private void Parse(string path)
        {
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var classList = JsonSerializer.Deserialize<List<Class>>(json, options)
                ?? throw new Exception($"Не удалось десериализовать JSON из файла {path}");

            Log($"Загружено {classList.Count} объектов из JSON.");

            // Первый проход: регистрируем все классы в словаре, замещая заглушки
            foreach (var cls in classList)
            {
                if (string.IsNullOrEmpty(cls.Name))
                    continue;
                _classes[cls.Name] = cls;
            }

            // Второй проход: разрешаем строковые ссылки в объекты
            foreach (var cls in _classes.Values.ToList())
            {
                // Разрешаем базовый класс
                if (!string.IsNullOrEmpty(cls.SubClassName))
                    cls.SubClass = GetOrCreate(cls.SubClassName);

                // Устанавливаем Domain на свойства и разрешаем Range
                foreach (var prop in cls.Properties.Values)
                {
                    prop.Domain = cls;

                    if (string.IsNullOrEmpty(prop.RangeName))
                        throw new Exception($"Свойство '{prop.Name}' класса '{cls.Name}' не имеет range. Range обязателен для всех свойств.");
                    prop.Range = GetOrCreate(prop.RangeName);
                }

                // Устанавливаем Domain на элементы перечислений
                foreach (var enumerator in cls.Enumerators.Values)
                    enumerator.Domain = cls;
            }

            // Третий проход: проверяем что у всех свойств заполнены Domain и Range
            foreach (var cls in _classes.Values)
            {
                foreach (var prop in cls.Properties.Values)
                {
                    if (prop.Domain == null)
                        throw new Exception($"Свойство '{prop.Name}' класса '{cls.Name}' имеет незаполненный Domain после парсинга.");
                    if (prop.Range == null)
                        throw new Exception($"Свойство '{prop.Name}' класса '{cls.Name}' имеет незаполненный Range после парсинга.");
                }
                foreach (var enumerator in cls.Enumerators.Values)
                {
                    if (enumerator.Domain == null)
                        throw new Exception($"Элемент перечисления '{enumerator.Name}' класса '{cls.Name}' имеет незаполненный Domain после парсинга.");
                }
            }

            Log($"После разрешения ссылок итого классов: {_classes.Count}");
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(JsonParse)}] {message}");
        }
    }
}
