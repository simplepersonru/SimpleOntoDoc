using System.IO;
using Xunit;

namespace SimpleOntoDoc.Tests
{
    /// <summary>
    /// Интеграционные тесты PlantUML.
    /// URL сервиса читается из переменной окружения SIMPLEDOC_PLANTUML_URL.
    /// Если переменная не задана, тесты пропускаются (Skip).
    /// В CI поднимается локальный Docker-контейнер plantuml/plantuml-server
    /// аналогично тому, как это делается в publish.bash.
    /// </summary>
    public class PlantUmlIntegrationTests
    {
        /// <summary>
        /// URL PlantUML-сервиса из переменной окружения. Null → тесты будут пропущены.
        /// </summary>
        private static readonly string? PlantUmlUrl =
            Environment.GetEnvironmentVariable("SIMPLEDOC_PLANTUML_URL");

        private static readonly string AnimalsJsonPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "animals.json");

        private static Dictionary<string, Class> ParseAnimals(string url)
        {
            var options = new Program.Options
            {
                InputJsonPath = AnimalsJsonPath,
                OutputPath = Path.GetTempPath(),
                DocTitle = "Animals PlantUML Test",
                DocDescription = "PlantUML integration test ontology",
                SkipPlantUml = false,
                PlantumlRemoteUrl = url,
            };
            return new JsonParse(options).Classes;
        }

        [SkippableFact]
        public async Task FillClassesAsync_AllClassTypeClasses_HaveSvgDiagram()
        {
            Skip.If(string.IsNullOrEmpty(PlantUmlUrl),
                "SIMPLEDOC_PLANTUML_URL не задан — PlantUML-тесты пропускаются.");

            Dictionary<string, Class> classes = ParseAnimals(PlantUmlUrl!);
            var plantuml = new PlantUML(PlantUmlUrl!);

            await plantuml.FillClassesAsync(classes);

            IEnumerable<Class> classTypeClasses = classes.Values.Where(c => c.Type == ClassType.Class);
            Assert.NotEmpty(classTypeClasses);

            foreach (Class cls in classTypeClasses)
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(cls.SvgDiagram),
                    $"Класс '{cls.Name}' имеет пустой SvgDiagram после генерации PlantUML.");
            }
        }

        [SkippableFact]
        public async Task FillClassesAsync_NonClassTypeEntities_HaveEmptySvgDiagram()
        {
            Skip.If(string.IsNullOrEmpty(PlantUmlUrl),
                "SIMPLEDOC_PLANTUML_URL не задан — PlantUML-тесты пропускаются.");

            Dictionary<string, Class> classes = ParseAnimals(PlantUmlUrl!);
            var plantuml = new PlantUML(PlantUmlUrl!);

            await plantuml.FillClassesAsync(classes);

            // Для не-Class типов SvgDiagram не генерируется
            IEnumerable<Class> nonClassTypeEntities = classes.Values
                .Where(c => c.Type != ClassType.Class);

            foreach (Class cls in nonClassTypeEntities)
            {
                Assert.True(
                    string.IsNullOrWhiteSpace(cls.SvgDiagram),
                    $"Сущность '{cls.Name}' типа {cls.Type} не должна иметь SvgDiagram.");
            }
        }

        [SkippableFact]
        public async Task FillClassesAsync_SvgDiagramContainsValidSvgContent()
        {
            Skip.If(string.IsNullOrEmpty(PlantUmlUrl),
                "SIMPLEDOC_PLANTUML_URL не задан — PlantUML-тесты пропускаются.");

            Dictionary<string, Class> classes = ParseAnimals(PlantUmlUrl!);
            var plantuml = new PlantUML(PlantUmlUrl!);

            await plantuml.FillClassesAsync(classes);

            foreach (Class cls in classes.Values.Where(c => c.Type == ClassType.Class))
            {
                Assert.Contains("<svg", cls.SvgDiagram, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
