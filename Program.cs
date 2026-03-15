namespace SimpleOntoDoc
{
    internal class Program
    {
        public class Options
        {
            /// <summary>Путь к входному JSON-файлу онтологии (формат model.py).</summary>
            public required string InputJsonPath { get; set; }

            /// <summary>URL удалённого сервера PlantUML. Может быть null, если SkipPlantUml = true.</summary>
            public string? PlantumlRemoteUrl { get; set; }

            public required string OutputPath { get; set; }
            public required string DocTitle { get; set; }
            public required string DocDescription { get; set; }
            /// <summary>
            /// Базовый путь сайта относительно домена (например, "/hello" для ky.ru/hello/...).
            /// Пустая строка означает корень домена. Всегда начинается с "/" если не пустой.
            /// </summary>
            public string BasePath { get; set; } = string.Empty;

            /// <summary>
            /// Если true — шаг генерации SVG-диаграмм PlantUML полностью пропускается.
            /// Устанавливается через ENV SIMPLEDOC_PLANTUML_SKIP=true.
            /// </summary>
            public bool SkipPlantUml { get; set; } = false;
        }

        static string GetEnv(string env)
        {
            string? val = Environment.GetEnvironmentVariable(env);
            if (val == null)
                throw new Exception($"Не определен ENV {env}");
            return val;
        }

        static string GetEnvOptional(string env, string defaultValue = "")
        {
            return Environment.GetEnvironmentVariable(env) ?? defaultValue;
        }

        static string NormalizeBasePath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return string.Empty;
            return "/" + basePath.Trim('/');
        }

        static async Task Main(string[] args)
        {
            Log("Запуск генерации документации...");

            bool skipPlantUml = Environment.GetEnvironmentVariable("SIMPLEDOC_PLANTUML_SKIP") == "true";

            using var plantumlDocker = skipPlantUml ? null : new PlantUmlDockerManager();

            var options = new Options
            {
                InputJsonPath = GetEnv("SIMPLEDOC_INPUT_PATH"),
                PlantumlRemoteUrl = skipPlantUml ? null : plantumlDocker!.RemoteUrl,
                OutputPath = GetEnv("SIMPLEDOC_OUTPUT_PATH"),
                DocTitle = GetEnv("SIMPLEDOC_TITLE"),
                DocDescription = GetEnv("SIMPLEDOC_DESCRIPTION"),
                BasePath = NormalizeBasePath(GetEnvOptional("SIMPLEDOC_BASE_PATH")),
                SkipPlantUml = skipPlantUml,
            };

            Log("Чтение и парсинг JSON схемы...");
            var classes = new JsonParse(options).Classes;

            if (!options.SkipPlantUml)
            {
                Log("Генерация SVG-диаграмм PlantUML...");
                var umlrender = new PlantUML(options.PlantumlRemoteUrl!);
                await umlrender.FillClassesAsync(classes);
            }
            else
            {
                Log("Генерация SVG-диаграмм PlantUML пропущена (SIMPLEDOC_PLANTUML_SKIP=true).");
            }

            Log("Генерация HTML-документации...");
            var generator = new SiteGenerator(
                data: classes,
                options: options);

            await generator.GenerateAsync();

            Log("Генерация завершена успешно.");
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(Program)}] {message}");
        }
    }
}
