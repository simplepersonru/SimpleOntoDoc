using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOntoDoc
{
    // ─── Вспомогательные расширения для Model-классов ────────────────────────────
    // Методы, которые зависят от Type/Namespace/Name, но не являются данными
    // из JSON-формата model.py, вынесены сюда из Model.cs.

    public static class ClassExtensions
    {
        public static string StereoPath(this Class cls) => cls.Type switch
        {
            ClassType.Datatype => "datatypes",
            ClassType.Enum => "enums",
            ClassType.Class => "classes",
            ClassType.Primitive => "primitives",
            ClassType.Compound => "compounds",
            _ => "classes"
        };

        public static string Link(this Class cls, bool near)
        {
            return near ? $"{cls.Name}.md" : $"./entities/{cls.Name}.md";
        }

        public static string MarkdownRef(this Class cls, bool near)
        {
            return $"[{cls.Id}]({cls.Link(near)})";
        }

        public static string MarkdownColorSquare(this Class cls) => cls.Type switch
        {
            ClassType.Class => "🟦",
            ClassType.Enum => "🟪",
            ClassType.Primitive => "🟧",
            ClassType.Datatype => "🟨",
            ClassType.Compound => "🟥",
            _ => "⬜"
        };

        public static string Href(this Class cls) => $"{cls.StereoPath()}/{cls.Name}.html";

        public static string BadgeClass(this Class cls) => BadgeClassStatic(cls.Type);

        public static string BadgeClassStatic(ClassType type) => type switch
        {
            ClassType.Class => "badge-class",
            ClassType.Enum => "badge-enum",
            ClassType.Primitive => "badge-primitive",
            ClassType.Datatype => "badge-datatype",
            ClassType.Compound => "badge-compound",
            _ => "badge-secondary"
        };

        public static string NamespacedId(this Class cls)
        {
            if (string.IsNullOrEmpty(cls.Namespace)
                || cls.Type == ClassType.Primitive
                || cls.Type == ClassType.Datatype)
                return cls.Name;
            return $"{cls.Namespace}:{cls.Name}";
        }
    }

    public static class PropertyExtensions
    {

        public static string MarkdownRef(this Property prop, bool useClassPart = true)
        {
            string link = prop.Domain.Link(near: true);
            string text = useClassPart ? prop.Id : prop.Name;

            return $"[{text}]({link}#{prop.Name})";
        }
        public static string Href(this Property prop) =>
            $"properties/{prop.Domain.Name}.{prop.Name}.html";

        public static string NameWithAnchor(this Property prop)
        {
            return $"<div id=\"{prop.Name}\"></div> {prop.MarkdownRef(false)}";
        }
        

        public static string MarkdownLimits(this Property prop)
        {
            StringBuilder sb = new();
            if (prop.Multiplicity != null)
                sb.Append($"_multiplicity_: {prop.Multiplicity}<br/> ");

            if (prop.Min.HasValue && !prop.Max.HasValue)
                sb.Append($"_≥_ {prop.Min.Value}<br/> ");
            if (!prop.Min.HasValue && prop.Max.HasValue)
                sb.Append($"_≤_ {prop.Max.Value}<br/> ");
            if (prop.Min.HasValue && prop.Max.HasValue)
                sb.Append($"_≥_ {prop.Min.Value}, _≤_ {prop.Max.Value}<br/> ");
                
            if (prop.Pattern != null)
                sb.Append($"_pattern_: {prop.Pattern}<br/> ");

            return sb.ToString();
        }

        public static string NamespacedId(this Property prop) =>
            string.IsNullOrEmpty(prop.Namespace) ? prop.Name : $"{prop.Namespace}:{prop.Name}";
    }

    public static class EnumeratorExtensions
    {
        public static string NameWithAnchor(this Enumerator prop)
        {
            return $"<div id=\"{prop.Name}\"></div> {prop.MarkdownRef(false)}";
        }
        public static string MarkdownRef(this Enumerator prop, bool useClassPart = false)
        {
            string link = prop.Domain.Link(near: true);
            string text = useClassPart ? prop.Id : prop.Name;

            return $"[{text}]({link}#{prop.Name})";
        }
    }

    // ─── View-модели для навигации ────────────────────────────────────────────────

    public class BreadcrumbItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class LayoutViewModel
    {
        public string Title { get; set; } = "SimpleOntoDoc";
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CurrentPage { get; set; } = "home";
        public int PropertyCount { get; set; }
        public int EnitityCount { get; set; }
        public string BasePath { get; set; } = string.Empty;
    }

    public class IndexViewModel : LayoutViewModel
    {
        public List<Class> ExampleClasses { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public List<Property> ExampleProperties { get; set; } = new();
        public int ClassCount { get; set; }
        public int EnumCount { get; set; }
        public int PrimitiveCount { get; set; }
        public int DataTypeCount { get; set; }
        public int CompoundCount { get; set; }
    }

    public class ClassViewModel : LayoutViewModel
    {
        public Class Class { get; set; } = new();
        public List<Property> Properties { get; set; } = new();
        public List<Class> ChildClasses { get; set; } = new();
        public List<Property> AllProperties { get; set; } = new();
    }

    public class PropertyViewModel : LayoutViewModel
    {
        public Property Property { get; set; } = new();
    }

    /// <summary>
    /// Модель для списка классов. Type == null означает «все сущности».
    /// </summary>
    public class ClassListViewModel : LayoutViewModel
    {
        public List<Class> Classes { get; set; } = new();
        /// <summary>null — показывать все типы (страница Entities).</summary>
        public ClassType? Type { get; set; }
    }

    public class PropertyListViewModel : LayoutViewModel
    {
        public List<Property> Properties { get; set; } = new();
    }

    public enum DiagramWay
    {
        Vanilla, ///< инлайн диаграмма внутри MD файла 
        Grammax ///< Вынесенная в отдельный puml файл
    }

    public class MarkdownIndexViewModel
    {
        public DiagramWay DiagramWay {get; set;}  
        public string Title { get; set; } = "SimpleOntoDoc";
        public string Description { get; set; } = string.Empty;
        public int ClassCount { get; set; }
        public int EnumCount { get; set; }
        public int PrimitiveCount { get; set; }
        public int DataTypeCount { get; set; }
        public int CompoundCount { get; set; }
        public List<Class> Classes { get; set; } = new();
        public string AllClassesDiagramContent { get; set; } = string.Empty;

    }

    public class MarkdownClassViewModel
    {
        public DiagramWay DiagramWay {get; set;}  
        public Class Class { get; set; } = new();
        public string SubClassString { get; set; } = "-";
        public List<Property> Properties { get; set; } = new();
        public List<Class> ChildClasses { get; set; } = new();
        public List<Property> LinkProperties { get; set; } = new();
        public List<Property> AllClassProperties { get; set; } = new();

    }
}
