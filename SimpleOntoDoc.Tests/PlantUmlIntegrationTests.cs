using System.IO;
using Xunit;

namespace SimpleOntoDoc.Tests
{
    /// <summary>
    /// Интеграционные тесты PlantUML.
    /// Использует публичный сервис https://plantuml.simpleperson.ru/ для генерации диаграмм.
    /// Проверяет, что для каждого класса типа ClassType.Class поле SvgDiagram заполняется
    /// после вызова PlantUML.FillClassesAsync.
    /// </summary>
    public class PlantUmlIntegrationTests
    {
        private const string PlantUmlUrl = "https://plantuml.simpleperson.ru/";

        private static readonly string AnimalsJsonPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "animals.json");

        private static Dictionary<string, Class> ParseAnimals()
        {
            var options = new Program.Options
            {
                InputJsonPath = AnimalsJsonPath,
                OutputPath = Path.GetTempPath(),
                DocTitle = "Animals PlantUML Test",
                DocDescription = "PlantUML integration test ontology",
                SkipPlantUml = false,
                PlantumlRemoteUrl = PlantUmlUrl,
            };
            return new JsonParse(options).Classes;
        }

        [Fact]
        public async Task FillClassesAsync_AllClassTypeClasses_HaveSvgDiagram()
        {
            Dictionary<string, Class> classes = ParseAnimals();
            var plantuml = new PlantUML(PlantUmlUrl);

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

        [Fact]
        public async Task FillClassesAsync_NonClassTypeEntities_HaveEmptySvgDiagram()
        {
            Dictionary<string, Class> classes = ParseAnimals();
            var plantuml = new PlantUML(PlantUmlUrl);

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

        [Fact]
        public async Task FillClassesAsync_SvgDiagramContainsValidSvgContent()
        {
            Dictionary<string, Class> classes = ParseAnimals();
            var plantuml = new PlantUML(PlantUmlUrl);

            await plantuml.FillClassesAsync(classes);

            foreach (Class cls in classes.Values.Where(c => c.Type == ClassType.Class))
            {
                Assert.Contains("<svg", cls.SvgDiagram, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
