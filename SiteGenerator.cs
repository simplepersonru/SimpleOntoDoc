using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleOntoDoc
{
    internal class SiteGenerator
    {
        private readonly RazorLightEngine _engine;
        private readonly Dictionary<string, Class> _data;
        private readonly List<Class> _classes;
        private readonly List<Property> _properties;
        private readonly Program.Options _options;
        private readonly string _basePath;

        public SiteGenerator(Dictionary<string, Class> data, Program.Options options, string? basePath = null)
        {
            _data = data;
            _options = options;
            _basePath = basePath ?? AppContext.BaseDirectory;

            Log("Инициализация движка RazorLight и подготовка данных...");

            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(_basePath, "templates"))
                .UseMemoryCachingProvider()
                .Build();

            // Разделяем данные по типам
            _classes = data.Values
                .Where(x => x.Type == ClassType.Class)
                .ToList();

            // Собираем все свойства
            _properties = data.Values
                .Where(x => x.Type == ClassType.Class)
                .SelectMany(x => x.Properties.Values)
                .ToList();

            // Очищаем и создаём структуру выходных директорий
            PrepareOutputDirectories();
        }

        private void PrepareOutputDirectories()
        {
            Log("Очистка и подготовка выходной директории...");
            if (Directory.Exists(_options.OutputPath))
                Directory.GetFiles(_options.OutputPath, "*", SearchOption.AllDirectories)
                        .ToList()
                        .ForEach(File.Delete);

            Directory.CreateDirectory(_options.OutputPath);
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "classes"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "properties"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "enums"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "primitives"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "datatypes"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "compounds"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "assets"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "assets", "js"));
            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "assets", "css"));
        }

        public async Task GenerateAsync()
        {
            Log("Генерация поискового индекса...");
            await GenerateSearchIndexAsync();

            Log("Генерация главной страницы...");
            await GenerateIndexAsync();

            Log("Генерация списков классов и свойств...");
            await GeneratePropertyListAsync();
            await GenerateClassListAsync(ClassType.Class);
            await GenerateClassListAsync(ClassType.Enum);
            await GenerateClassListAsync(ClassType.Primitive);
            await GenerateClassListAsync(ClassType.Datatype);
            await GenerateClassListAsync(ClassType.Compound);
            await GenerateClassListAsync(null); // All entities

            Log("Генерация страниц классов...");
            foreach (var cls in _data.Values)
            {
                await GenerateClassPageAsync(cls);
            }

            Log("Генерация страниц свойств...");
            foreach (var prop in _properties)
            {
                await GeneratePropertyPageAsync(prop);
            }

            Log("Генерация завершена.");
        }

        private string AbsoluteUrl(string relativePath) =>
            string.IsNullOrEmpty(_options.BasePath)
                ? $"/{relativePath}"
                : $"{_options.BasePath}/{relativePath}";

        private async Task GeneratePropertyPageAsync(Property prop)
        {
            var model = new PropertyViewModel
            {
                Title = prop.Name,
                Property = prop,
                CurrentPage = "properties",
                PropertyCount = _properties.Count,
                EnitityCount = _data.Count,
                BasePath = _options.BasePath,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = AbsoluteUrl("index.html") },
                    new() { Name = "Properties", Url = AbsoluteUrl("properties/_index.html") },
                    new() { Name = prop.Id, Url = AbsoluteUrl(prop.Href()) }
                }
            };

            string html = await _engine.CompileRenderAsync("Property.cshtml", model);
            await WriteOutputAsync(prop.Href(), html);
        }

        private string stereoPath(ClassType? type) => type.HasValue
            ? new Class { Type = type.Value }.StereoPath()
            : "entities";

        private string nameList(ClassType? type) => type switch
        {
            ClassType.Class => "Classes",
            ClassType.Enum => "Enums",
            ClassType.Primitive => "Primitives",
            ClassType.Datatype => "DataTypes",
            ClassType.Compound => "Compounds",
            null => "Entities",
            _ => "Classes"
        };

        private async Task GenerateClassPageAsync(Class cls)
        {
            var properties = _properties.Where(p => p.Domain.Id == cls.Id).ToList();

            var childClasses = _classes.Where(c => c.SubClass?.Id == cls.Id).ToList();

            var model = new ClassViewModel
            {
                Title = cls.Name,
                Class = cls,
                Properties = properties,
                ChildClasses = childClasses,
                CurrentPage = cls.StereoPath(),
                EnitityCount = _data.Count,
                PropertyCount = _properties.Count,
                AllProperties = _properties,
                BasePath = _options.BasePath,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = AbsoluteUrl("index.html") },
                    new() { Name = nameList(cls.Type), Url = AbsoluteUrl($"{cls.StereoPath()}/_index.html") },
                    new() { Name = cls.Name, Url = AbsoluteUrl(cls.Href()) }
                }
            };

            string html = await _engine.CompileRenderAsync("Class.cshtml", model);
            await WriteOutputAsync(cls.Href(), html);
        }

        private async Task GenerateSearchIndexAsync()
        {
            Log("Генерация JSON-индекса для поиска...");
            var searchData = new
            {
                Classes = _data.Values.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    url = AbsoluteUrl(c.Href()),
                    type = c.Type.ToString(),
                    description = c.Description,
                    stereotype = c.Type.ToString()
                }),
                Properties = _properties.Select(p => new
                {
                    id = $"{p.Domain.Name}.{p.Name}",
                    name = p.Name,
                    url = AbsoluteUrl(p.Href()),
                    type = "Property",
                    description = $"{p.Domain.Name} → {p.Range.Name}",
                    domain = p.Domain.Name,
                    range = p.Range.Name
                }),
            };

            string json = JsonSerializer.Serialize(searchData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await WriteOutputAsync("assets/search-index.json", json);

            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "assets", "js"));
            File.Copy(Path.Combine(_basePath, "assets", "js", "search.js"),
                      Path.Combine(_options.OutputPath, "assets", "js", "search.js"),
                      overwrite: true);

            Directory.CreateDirectory(Path.Combine(_options.OutputPath, "assets", "css"));
            File.Copy(Path.Combine(_basePath, "assets", "css", "site.css"),
                      Path.Combine(_options.OutputPath, "assets", "css", "site.css"),
                      overwrite: true);
        }

        private async Task GenerateIndexAsync()
        {
            var model = new IndexViewModel
            {
                Title = _options.DocTitle,
                Description = _options.DocDescription,
                ExampleClasses = _classes.Take(5).ToList(),
                ExampleProperties = _properties.Take(5).ToList(),
                CurrentPage = "home",
                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
                EnumCount = _data.Values.Count(x => x.Type == ClassType.Enum),
                PrimitiveCount = _data.Values.Count(x => x.Type == ClassType.Primitive),
                DataTypeCount = _data.Values.Count(x => x.Type == ClassType.Datatype),
                CompoundCount = _data.Values.Count(x => x.Type == ClassType.Compound),
                EnitityCount = _data.Count,
                BasePath = _options.BasePath,
            };

            string html = await _engine.CompileRenderAsync("Index.cshtml", model);
            await WriteOutputAsync("index.html", html);
        }

        private async Task GenerateClassListAsync(ClassType? type)
        {
            Log($"Генерация списка классов для типа {type?.ToString() ?? "All"}...");
            List<Class> classes;
            if (type == null)
                classes = _data.Values
                            .OrderBy(c => c.Id)
                            .ToList();
            else
                classes = _data.Values
                            .Where(c => c.Type == type)
                            .OrderBy(c => c.Id)
                            .ToList();

            string url;
            if (type == null)
                url = "entities.html";
            else
                url = $"{stereoPath(type)}/_index.html";

            var model = new ClassListViewModel
            {
                Title = nameList(type),
                Classes = classes,
                Type = type,
                CurrentPage = stereoPath(type),
                EnitityCount = _data.Count,
                PropertyCount = _properties.Count,
                BasePath = _options.BasePath,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = AbsoluteUrl("index.html") },
                    new() { Name = nameList(type), Url = AbsoluteUrl(url) }
                }
            };

            string html = await _engine.CompileRenderAsync("ClassList.cshtml", model);
            await WriteOutputAsync(url, html);
        }

        private async Task GeneratePropertyListAsync()
        {
            Log("Генерация списка всех свойств...");
            var model = new PropertyListViewModel
            {
                Title = "All Properties",
                Properties = _properties.OrderBy(p => p.Id).ToList(),
                CurrentPage = "properties",
                EnitityCount = _data.Count,
                PropertyCount = _properties.Count,
                BasePath = _options.BasePath,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = AbsoluteUrl("index.html") },
                    new() { Name = "Properties", Url = AbsoluteUrl("properties/_index.html") }
                }
            };

            string html = await _engine.CompileRenderAsync("PropertyList.cshtml", model);
            await WriteOutputAsync("properties/_index.html", html);
        }

        private async Task WriteOutputAsync(string relativePath, string content)
        {
            string fullPath = Path.Combine(_options.OutputPath, relativePath);
            string? directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(SiteGenerator)}] {message}");
        }
    }
}

