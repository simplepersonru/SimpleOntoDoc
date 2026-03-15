using System.IO;
using Xunit;

namespace SimpleOntoDoc.Tests
{
    /// <summary>
    /// Интеграционные тесты генерации сайта без PlantUML (SkipPlantUml = true).
    /// Проверяет, что в выходной директории создаётся правильное количество файлов
    /// для каждого типа сущностей (классы, перечисления, свойства и т.д.).
    /// Временные файлы удаляются после завершения каждого теста.
    /// </summary>
    public class SiteGeneratorIntegrationTests : IDisposable
    {
        private static readonly string AnimalsJsonPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "animals.json");

        private readonly string _outputPath;
        private readonly Dictionary<string, Class> _classes;

        public SiteGeneratorIntegrationTests()
        {
            _outputPath = Path.Combine(Path.GetTempPath(), $"SimpleOntoDocTest_{Guid.NewGuid():N}");

            var options = new Program.Options
            {
                InputJsonPath = AnimalsJsonPath,
                OutputPath = _outputPath,
                DocTitle = "Animals Test Ontology",
                DocDescription = "Integration test ontology",
                SkipPlantUml = true,
            };

            _classes = new JsonParse(options).Classes;

            string basePath = AppContext.BaseDirectory;
            var generator = new SiteGenerator(_classes, options, basePath);
            generator.GenerateAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            // Удаляем временные файлы после каждого теста
            if (Directory.Exists(_outputPath))
                Directory.Delete(_outputPath, recursive: true);
        }

        [Fact]
        public void GenerateAsync_OutputDirectoryCreated()
        {
            Assert.True(Directory.Exists(_outputPath));
        }

        [Fact]
        public void GenerateAsync_IndexHtmlExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "index.html")));
        }

        [Fact]
        public void GenerateAsync_EntitiesHtmlExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "entities.html")));
        }

        [Fact]
        public void GenerateAsync_SearchIndexExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "assets", "search-index.json")));
        }

        [Fact]
        public void GenerateAsync_CssFileExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "assets", "css", "site.css")));
        }

        [Fact]
        public void GenerateAsync_JsFileExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "assets", "js", "search.js")));
        }

        [Fact]
        public void GenerateAsync_ClassListIndexExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "classes", "_index.html")));
        }

        [Fact]
        public void GenerateAsync_EnumListIndexExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "enums", "_index.html")));
        }

        [Fact]
        public void GenerateAsync_PropertyListIndexExists()
        {
            Assert.True(File.Exists(Path.Combine(_outputPath, "properties", "_index.html")));
        }

        /// <summary>
        /// Данные для параметризованного теста подсчёта страниц по типу сущности.
        /// Формат: (ClassType, папка в output).
        /// </summary>
        public static IEnumerable<object[]> EntityTypeDirectories =>
        [
            [ClassType.Class,     "classes"],
            [ClassType.Enum,      "enums"],
            [ClassType.Primitive, "primitives"],
            [ClassType.Datatype,  "datatypes"],
            [ClassType.Compound,  "compounds"],
        ];

        /// <summary>
        /// Для каждого типа сущности проверяет, что количество HTML-файлов
        /// в соответствующей папке (кроме _index.html) совпадает с количеством
        /// объектов этого типа в онтологии.
        /// </summary>
        [Theory]
        [MemberData(nameof(EntityTypeDirectories))]
        public void GenerateAsync_EntityPageCountMatchesTypeCount(ClassType type, string folder)
        {
            int expected = _classes.Values.Count(c => c.Type == type);
            string dir = Path.Combine(_outputPath, folder);
            int actual = Directory.GetFiles(dir, "*.html")
                .Count(f => Path.GetFileName(f) != "_index.html");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GenerateAsync_PropertyPageCountMatchesPropertiesOfClassTypeClasses()
        {
            int expectedPropertyCount = _classes.Values
                .Where(c => c.Type == ClassType.Class)
                .SelectMany(c => c.Properties.Values)
                .Count();

            string propDir = Path.Combine(_outputPath, "properties");
            int actualPropertyFiles = Directory.GetFiles(propDir, "*.html")
                .Where(f => Path.GetFileName(f) != "_index.html")
                .Count();

            Assert.Equal(expectedPropertyCount, actualPropertyFiles);
        }

        [Fact]
        public void GenerateAsync_ClassPagesHaveExpectedNames()
        {
            string classDir = Path.Combine(_outputPath, "classes");
            string[] classFiles = Directory.GetFiles(classDir, "*.html")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => n != "_index")
                .ToArray();

            // LivingBeing, Animal, Mammal, Dog, Person
            Assert.Contains("LivingBeing", classFiles);
            Assert.Contains("Animal", classFiles);
            Assert.Contains("Mammal", classFiles);
            Assert.Contains("Dog", classFiles);
            Assert.Contains("Person", classFiles);
        }

        [Fact]
        public void GenerateAsync_ClassPagesDoNotHaveSvgDiagramContent()
        {
            // PlantUML отключён, SvgDiagram должен быть пустым
            foreach (Class cls in _classes.Values.Where(c => c.Type == ClassType.Class))
                Assert.Empty(cls.SvgDiagram);
        }

        [Fact]
        public void GenerateAsync_TotalHtmlFileCount()
        {
            // 1 index + 1 entities + 5 list pages (_index for each type + properties) + all entity pages + property pages
            int classPages = _classes.Values.Count(c => c.Type == ClassType.Class);
            int enumPages = _classes.Values.Count(c => c.Type == ClassType.Enum);
            int primitivePages = _classes.Values.Count(c => c.Type == ClassType.Primitive);
            int datatypePages = _classes.Values.Count(c => c.Type == ClassType.Datatype);
            int compoundPages = _classes.Values.Count(c => c.Type == ClassType.Compound);
            int propertyPages = _classes.Values
                .Where(c => c.Type == ClassType.Class)
                .SelectMany(c => c.Properties.Values)
                .Count();

            // 6 list pages: classes/_index, enums/_index, primitives/_index,
            //               datatypes/_index, compounds/_index, properties/_index
            // 1 entities.html, 1 index.html
            int expectedTotal = 1 + 1 + 6 + classPages + enumPages + primitivePages
                                + datatypePages + compoundPages + propertyPages;

            int actualTotal = Directory.GetFiles(_outputPath, "*.html", SearchOption.AllDirectories).Length;

            Assert.Equal(expectedTotal, actualTotal);
        }
    }
}
