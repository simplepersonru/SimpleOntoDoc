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
            using var plantumlDocker = new PlantUmlDockerManager();
            var options = new Options
			{
				// Разделитель путей: Path.PathSeparator (обычно ; на Windows, : на Linux)
				RdfsPaths = GetEnv("RDFSDOC_PATH_TO_RDFS")
					.Split(new[] { System.IO.Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries),
				PlantumlRemoteUrl = plantumlDocker.RemoteUrl,
				OutputPath = GetEnv("RDFSDOC_OUTPUT_PATH"),
				DocTitle = GetEnv("RDFSDOC_TITLE"),
				DocDescription = GetEnv("RDFSDOC_DESCRIPTION"),
				CommonNamespace = GetEnv("RDFSDOC_COMMON_NAMESPACE"),
				UseNamespaceForProperties = Convert.ToBoolean(Environment.GetEnvironmentVariable("RDFSDOC_USE_NAMESPACE_FOR_PROPERTIES"))
			};

			//var options = new Options
			//{
			//	RdfsPaths = new[] { "C:\\reposroot\\redkit-lab\\dmsutils\\cimparser\\scripts\\ck-rdf.xml" },
			//	PlantumlRemoteUrl = "http://localhost:55555",
			//	OutputPath = "output",
			//	DocTitle = "Example",
			//	DocDescription = "Example descr",
			//	CommonNamespace = "cim",
			//	UseNamespaceForProperties = false
			//};

			var classes = new XmlParse(options).Classes;

			var umlrender = new PlantUML(options.PlantumlRemoteUrl);
			await umlrender.FillClassesAsync(classes);

			var generator = new SiteGenerator(
				data: classes,
				options: options);

			await generator.GenerateAsync();
		}
	}
}
