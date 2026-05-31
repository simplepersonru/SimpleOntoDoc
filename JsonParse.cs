using System.Text.Json;
using System.Text.Json.Serialization;
using Hjson;

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
            var json = HjsonValue.Load(path).ToString();

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

                cls.Properties = cls.PropertiesList?.ToDictionary(p => p.Name) ?? new Dictionary<string, Property>();
                cls.Enumerators = cls.EnumeratorsList?.ToDictionary(e => e.Name) ?? new Dictionary<string, Enumerator>();

                foreach (var rel in cls.Relations ?? new List<Relation>())
                {
                    if (string.IsNullOrEmpty(rel.LeftName) || string.IsNullOrEmpty(rel.RightName))
                        throw new Exception($"Отношение в классе '{cls.Name}' имеет незаполненные LeftName или RightName. Оба поля обязательны для всех отношений.");
                    rel.Left = GetOrCreate(rel.LeftName);
                    rel.Right = GetOrCreate(rel.RightName);
                }

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
                    if (string.IsNullOrEmpty(prop.Domain.Name))
                        throw new Exception($"Свойство '{prop.Name}' класса '{cls.Name}' имеет незаполненный Domain после парсинга.");
                    if (string.IsNullOrEmpty(prop.Range.Name))
                        throw new Exception($"Свойство '{prop.Name}' класса '{cls.Name}' имеет незаполненный Range после парсинга.");
                }
                foreach (var enumerator in cls.Enumerators.Values)
                {
                    if (string.IsNullOrEmpty(enumerator.Domain.Name))
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
