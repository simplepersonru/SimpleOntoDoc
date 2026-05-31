using PlantUml.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleOntoDoc
{
    class PlantUmlBuilder(Program.Options _options)
    {
        StringBuilder _decl = new();
        StringBuilder _main = new();
        HashSet<string> _declaredClasses = new();
        HashSet<string> _declaredParentClasses = new();
        HashSet<string> _declaredEnums = new();


        private string AbsoluteUrl(string relativePath) =>
          string.IsNullOrEmpty(_options.BasePath)
              ? $"/{relativePath}"
              : $"{_options.BasePath}/{relativePath}";

        static string PlantUmlId(Class cls)
        {
            return $"{cls.Namespace}.{cls.Name}";
        }

        void ParentClass(Class cls, bool first = true)
        {
            if (!_declaredParentClasses.Add(PlantUmlId(cls)))
            {
                return;
            }

            if (!first)
                Class(cls);

            if (cls.SubClass == null)
                return;

            _main.AppendLine($"{PlantUmlId(cls.SubClass)} <|-down- {PlantUmlId(cls)}");
            ParentClass(cls.SubClass, first: false);
        }
        void Enum(Class cls)
        {
            if (!_declaredEnums.Add(PlantUmlId(cls)))
                return;

            string link = _options.MarkdownRender ? string.Empty : $"[[{AbsoluteUrl(cls.Href())}]]";
            string viewString = string.IsNullOrEmpty(cls.Display) ? cls.Id : $"{cls.Id}\\n{cls.Display}";

            _decl.AppendLine($"""enum "{viewString}" as {PlantUmlId(cls)} {link} """);

            _main.AppendLine($"enum {PlantUmlId(cls)} {{");
            foreach (var descr in cls.Enumerators)
            {
                _main.AppendLine($"#{descr.Value.Name}");
            }
            _main.AppendLine("}");

        }
        void ClassRelations(Class cls, bool allClassesFlag = false)
        {
            foreach (var rel in cls.Relations ?? [])
            {
                _main.AppendLine($"{PlantUmlId(cls)} {rel.RelationLine} {PlantUmlId(rel.Right)}");
            }

            foreach (var propKeyValue in cls.Properties)
            {
                var prop = propKeyValue.Value;
                var type = prop.Range.Type;

                if (type != ClassType.Enum && type != ClassType.Class)
                    continue;

                if (allClassesFlag && type != ClassType.Class)
                    continue;

                if (type == ClassType.Enum && !allClassesFlag)
                    Enum(prop.Range);
                else if (type == ClassType.Class && !allClassesFlag)
                    Class(prop.Range, useProperties: false);

                string leftPart = PlantUmlId(cls);
                if (!allClassesFlag)
                    leftPart += $"::{prop.Name}";

                string relationSymbol = "--";
                if (!string.IsNullOrEmpty(prop.RelationLine) )
                {
                    relationSymbol = $"{prop.RelationLine}\"{prop.Multiplicity}\"";
                }

                _main.AppendLine($"{leftPart} {relationSymbol} {PlantUmlId(prop.Range)}");
            }
        }
        void Class(Class cls, bool useProperties = true)
        {
            if (!_declaredClasses.Add(PlantUmlId(cls)))
                return;

            string link = _options.MarkdownRender ? string.Empty : $"[[{AbsoluteUrl(cls.Href())}]]";
            string viewString = string.IsNullOrEmpty(cls.Display) ? cls.Id : $"{cls.Id}\\n{cls.Display}";

            _decl.AppendLine($"""class "{viewString}" as {PlantUmlId(cls)} {link} """);

            if (!useProperties)
                return;

            foreach (var prop in cls.Properties)
            {
                string rangeModifier = prop.Value.Range.Type switch
                {
                    ClassType.Class => "~",
                    ClassType.Enum => "#",
                    _ => "+",
                };

                string propNameInDiagram = _options.UseNamespaceInDiagramm ? prop.Value.NamespacedId() : prop.Value.Name;
                _main.AppendLine($"{PlantUmlId(cls)} : {rangeModifier}{propNameInDiagram} : {prop.Value.Range.Id}");
            }
        }

        string Result()
        {
            var result = new StringBuilder();
            result.Append(_decl.ToString());
            result.Append(_main.ToString());

            return result.ToString();
        }

        public string Build(Class cls)
        {
            _decl.AppendLine("skinparam groupInheritance 6");
            _decl.AppendLine("set separator none");
            _decl.AppendLine(
                        """
						annotation "Легенда" {
						  #ссылка на enum
						  ~ссылка на класс
						  +простое свойство
						}
						""");

            Class(cls, useProperties: true);
            ClassRelations(cls);
            ParentClass(cls);

            return Result();
        }

        public string BuildAllClasses(Dictionary<string, Class> data)
        {
            _decl.AppendLine("skinparam groupInheritance 6");
            _decl.AppendLine("set separator none");
            foreach (var cls in data.Values)
            {
                if (cls.Type == ClassType.Class)
                {
                    Class(cls, useProperties: false);
                    ClassRelations(cls, allClassesFlag: true);
                    ParentClass(cls);
                }

            }

            return Result();
        }
    }


    internal class PlantUML(Program.Options options)
    {
        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(PlantUML)}] {message}");
        }
        private async Task RenderClass(Class cls)
        {
            if (cls.Type != ClassType.Class)
                return;

            string diagramSource = new PlantUmlBuilder(options).Build(cls);
            if (options.MarkdownRender)
            {
                diagramSource = $"""
                                @startuml
                                {diagramSource}
                                @enduml
                                """;
                cls.DiagramContent = diagramSource;
                return;
            }

            var factory = new RendererFactory();
            var renderer = factory.CreateRenderer(new PlantUmlSettings { RemoteUrl = options.PlantumlRemoteUrl });
            var bytes = await renderer.RenderAsync(diagramSource, OutputFormat.Svg);
            cls.DiagramContent = Encoding.UTF8.GetString(bytes);
        }

        public string RenderAllClasses(Dictionary<string, Class> data)
        {
            string diagramSource = new PlantUmlBuilder(options).BuildAllClasses(data);

            if (!options.MarkdownRender)
                return diagramSource;

            return $"""
                    @startuml
                    {diagramSource}
                    @enduml
                    """;
        }

        public async Task FillClassesAsync(Dictionary<string, Class> data)
        {
            foreach (var cls in data)
            {
                await RenderClass(cls.Value);
            }
            return;

            // Иногда не работает и падает с исключением
            var throttler = new SemaphoreSlim(5);

            var tasks = data.Values.Select(async v =>
            {
                await throttler.WaitAsync();
                try
                {
                    await RenderClass(v);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Менеджер запуска контейнера PlantUML через Docker.
    /// На Windows все docker-команды выполняются через WSL (если ОС Windows).
    /// На Linux/macOS — напрямую.
    /// </summary>
    internal class PlantUmlDockerManager : IDisposable
    {
        private readonly string _containerName = "plantuml-server-rdfdoc-55667";
        private bool _startedByUs = false;
        public string RemoteUrl { get; }
        private readonly int _port;
        private readonly bool _useWsl;
        private readonly string _wslDistro;

        public PlantUmlDockerManager()
        {
            _port = 55667;
            _wslDistro = "Ubuntu-24.04";
            _useWsl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            RemoteUrl = $"http://localhost:{_port}";

            Log($"Инициализация менеджера Docker. Порт: {_port}, WSL: {_useWsl}");

            EnsureWslIfNeeded();

            if (!IsContainerRunning())
            {
                Log("Контейнер PlantUML не найден, запускаем новый контейнер...");
                StartContainer();
                _startedByUs = true;
                Log("Контейнер PlantUML успешно запущен.");
            }
            else
            {
                Log("Контейнер PlantUML уже запущен.");
            }
        }

        /// <summary>
        /// Если мы на Windows — убедиться, что WSL доступна и запущена.
        /// </summary>
        private void EnsureWslIfNeeded()
        {
            if (!_useWsl)
                return;

            Log("Проверка наличия и статуса WSL...");

            // Проверяем, что wsl.exe доступна
            try
            {
                var psi = new ProcessStartInfo("wsl", "--status")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var proc = Process.Start(psi);
                proc!.WaitForExit(3000);
                if (proc.ExitCode != 0)
                    throw new Exception("WSL не установлена или не настроена.");
            }
            catch (Exception ex)
            {
                Log("WSL не установлена или не настроена.");
                throw new Exception("WSL не установлена или не настроена.", ex);
            }

            // Запускаем дистрибутив, если он не запущен (wsl --list --running)
            var checkPsi = new ProcessStartInfo("wsl", "--list --running")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var proc = Process.Start(checkPsi))
            {
                string output = proc!.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                if (!output.Contains(_wslDistro, StringComparison.OrdinalIgnoreCase))
                {
                    Log($"Дистрибутив WSL '{_wslDistro}' не запущен. Запускаем...");
                    // Запускаем дистрибутив (wsl -d <distro> -e true)
                    var startPsi = new ProcessStartInfo("wsl", $"-d {_wslDistro} -e true")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    using var startProc = Process.Start(startPsi);
                    startProc!.WaitForExit();
                    Log($"Дистрибутив WSL '{_wslDistro}' запущен.");
                }
                else
                {
                    Log($"Дистрибутив WSL '{_wslDistro}' уже запущен.");
                }
            }
        }

        private ProcessStartInfo DockerPsi(string args)
        {
            if (_useWsl)
            {
                // Все docker-команды через wsl -d <distro> docker ...
                return new ProcessStartInfo("wsl", $"-d {_wslDistro} docker {args}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
            }
            else
            {
                return new ProcessStartInfo("docker", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
            }
        }

        private bool IsContainerRunning()
        {
            var psi = DockerPsi($"ps --filter name={_containerName} --format \"{{{{.Names}}}}\"");
            using var proc = Process.Start(psi);
            string output = proc!.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return output.Contains(_containerName);
        }

        private void StartContainer()
        {
            var psi = DockerPsi($"run -d --rm --name {_containerName} -p {_port}:8080 plantuml/plantuml-server:jetty");
            using var proc = Process.Start(psi);
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
                throw new Exception("Не удалось запустить контейнер PlantUML: " + proc.StandardError.ReadToEnd());
        }


        public void Dispose()
        {
            if (_startedByUs)
            {
                Log("Останавливаем контейнер PlantUML...");
                var psi = DockerPsi($"stop {_containerName}");
                using var proc = Process.Start(psi);
                proc!.WaitForExit();
                Log("Контейнер PlantUML остановлен.");
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{nameof(PlantUmlDockerManager)}] {message}");
        }
    }
}
