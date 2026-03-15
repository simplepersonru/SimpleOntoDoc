namespace SimpleOntoDoc
{
    internal class Program
    {
        public class Options
        {
            /// <summary>Путь к входному JSON-файлу онтологии (формат model.py).</summary>
            public required string InputJsonPath { get; set; }
            public required string PlantumlRemoteUrl { get; set; }
            public required string OutputPath { get; set; }
            public required string DocTitle { get; set; }
            public required string DocDescription { get; set; }
        }

        static string GetEnv(string env)
        {
            string? val = Environment.GetEnvironmentVariable(env);
            if (val == null)
                throw new Exception($"Не определен ENV {env}");
            return val;
        }

        static async Task Main(string[] args)
        {
            Log("Запуск генерации документации...");

            using var plantumlDocker = new PlantUmlDockerManager();

            var options = new Options
            {
                InputJsonPath = GetEnv("SIMPLEDOC_INPUT_PATH"),
                PlantumlRemoteUrl = plantumlDocker.RemoteUrl,
                OutputPath = GetEnv("SIMPLEDOC_OUTPUT_PATH"),
                DocTitle = GetEnv("SIMPLEDOC_TITLE"),
                DocDescription = GetEnv("SIMPLEDOC_DESCRIPTION"),
            };

            Log("Чтение и парсинг JSON схемы...");
            var classes = new JsonParse(options).Classes;

            Log("Генерация SVG-диаграмм PlantUML...");
            var umlrender = new PlantUML(options.PlantumlRemoteUrl);
            await umlrender.FillClassesAsync(classes);

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
