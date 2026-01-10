namespace RdfsBeautyDoc
{
	internal class Program
	{
		public class Options
		{
			// Теперь массив путей
			public required string[] RdfsPaths { get; set; }
			public required string PlantumlRemoteUrl { get; set; }
			public required string OutputPath { get; set; }
			public required string DocTitle { get; set; }
			public required string DocDescription { get; set; }
			public required string CommonNamespace { get; set; }
			public bool UseNamespaceForProperties { get; set; } = false;
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

            Log("Чтение и парсинг RDF/XML схемы...");
            var options = new Options
            {
                RdfsPaths = [ "C:\\reposroot\\redkit-lab\\dmsutils\\cimparser\\scripts\\ck-rdf.xml"],
                PlantumlRemoteUrl = plantumlDocker.RemoteUrl,
                OutputPath = "output",
                DocTitle = "Example",
                DocDescription = "Example descr",
                CommonNamespace = "cim",
                UseNamespaceForProperties = false
            };

            var classes = new XmlParse(options).Classes;

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
