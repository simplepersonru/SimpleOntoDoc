using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOntoDoc
{
    internal class MarkdownGenerator
    {
        private readonly RazorLightEngine _engine;
        private readonly Dictionary<string, Class> _data;
        private readonly List<Class> _classes;
        private readonly List<Property> _properties;
        private readonly Program.Options _options;
        private readonly string _basePath;

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(MarkdownGenerator)}] {message}");
        }

        public MarkdownGenerator(
            Dictionary<string, Class> data,
            Program.Options options)
        {
            _data = data;
            _options = options;
            _basePath = AppContext.BaseDirectory;

            Log("Инициализация движка RazorLight и подготовка данных...");

            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(_basePath, "templates"))
                .UseMemoryCachingProvider()
                .DisableEncoding()
                .Build();

            // Разделяем данные по типам
            _classes = data.Values
                .Where(x => x.Type == ClassType.Class)
                .ToList();

            // Собираем все свойства
            _properties = data.Values
                .Where(x => x.Type == ClassType.Class || x.Type == ClassType.Compound)
                .SelectMany(x => x.Properties.Values)
                .ToList();

            // Очищаем и создаём структуру выходных директорий
            PrepareOutputDirectories();
        }

        private void PrepareOutputDirectories()
        {
            Log("подготовка выходной директории...");

            Directory.CreateDirectory(_options.OutputPath);
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "entities"));
        }

        public async Task GenerateAsync()
        {
            Log("Генерация главной страницы...");
            await GenerateIndexAsync();

            Log("Генерация страниц классов...");
            foreach (var cls in _data.Values)
            {
                await GenerateClassPageAsync(cls);
            }

            Log("Генерация завершена.");
        }

        private async Task GenerateIndexAsync()
        {
            var model = new MarkdownIndexViewModel
            {
                Title = _options.DocTitle,
                Description = _options.DocDescription,
                Classes = _data.Values.ToList(),
                ClassCount = _classes.Count,
                EnumCount = _data.Values.Count(x => x.Type == ClassType.Enum),
                PrimitiveCount = _data.Values.Count(x => x.Type == ClassType.Primitive),
                DataTypeCount = _data.Values.Count(x => x.Type == ClassType.Datatype),
                CompoundCount = _data.Values.Count(x => x.Type == ClassType.Compound),
                AllClassesDiagramContent = new PlantUML(_options).RenderAllClasses(_data),
                DiagramWay = _options.DiagramWay
            };

            string markdown = await _engine.CompileRenderAsync("Index_md.cshtml", model);
            await WriteOutputAsync("Readme.md", markdown);
            if (_options.DiagramWay == DiagramWay.Grammax)
            {
                await WriteOutputAsync("index.puml", model.AllClassesDiagramContent);
            }
        }

        private async Task GenerateClassPageAsync(Class cls)
        {
            var properties = _properties.Where(p => p.Domain.Id == cls.Id).ToList();

            var childClasses = _classes.Where(c => c.SubClass?.Id == cls.Id).ToList();

            var allClassProperties = properties;
            var parent = cls.SubClass;
            while (parent != null)
            {
                var parentProperties = _properties.Where(p => p.Domain.Id == parent.Id).ToList();
                allClassProperties = parentProperties.Concat(allClassProperties).ToList();
                parent = parent.SubClass;
            }

            var model = new MarkdownClassViewModel
            {
                Class = cls,
                Properties = properties,
                SubClassString = cls.SubClass == null ? "-" : cls.SubClass.MarkdownRef(near: true),
                ChildClasses = childClasses,
                AllClassProperties = allClassProperties,
                LinkProperties = _properties.Where(p => p.Range.Id == cls.Id).ToList(),
                DiagramWay = _options.DiagramWay
            };

            string html = await _engine.CompileRenderAsync("Class_md.cshtml", model);
            await WriteOutputAsync($"entities/{cls.Id}.md", html);
            if (_options.DiagramWay == DiagramWay.Grammax)
            {
                await WriteOutputAsync($"entities/diagrams/{cls.Id}.puml", cls.DiagramContent);
            }
        }

        private async Task WriteOutputAsync(string relativePath, string content)
        {
            string fullPath = Path.Combine(_options.OutputPath, relativePath);
            string? directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
        }

    }
}
