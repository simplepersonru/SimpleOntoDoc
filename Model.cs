using System.Text.Json.Serialization;

namespace SimpleOntoDoc
{
    /// <summary>
    /// Базовый класс для всех сущностей онтологической схемы.
    /// Поля соответствуют структуре Base из model.py.
    /// </summary>
    public abstract class Base
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        public virtual string Id => Name;
    }

    /// <summary>
    /// Представляет свойство (атрибут или связь) класса в онтологической схеме.
    /// Соответствует классу Property из model.py.
    /// </summary>
    public class Property : Base
    {
        /// <summary>Имя класса-типа свойства (строковая ссылка из JSON, разрешается в Range после парсинга).</summary>
        [JsonPropertyName("range")]
        public string? RangeName { get; set; }

        [JsonPropertyName("optional")]
        public bool Optional { get; set; } = true;

        [JsonPropertyName("multiplicity")]
        public string? Multiplicity { get; set; }

        [JsonPropertyName("inverse_role_name")]
        public string? InverseRoleName { get; set; }

        /// <summary>Класс-владелец свойства (устанавливается после парсинга, не из JSON).</summary>
        [JsonIgnore]
        public Class? Domain { get; set; }

        /// <summary>Класс-тип свойства (разрешается из RangeName после парсинга).</summary>
        [JsonIgnore]
        public Class? Range { get; set; }

        public override string Id => $"{Domain?.Name}.{Name}";
    }

    /// <summary>
    /// Представляет элемент перечисления в онтологической схеме.
    /// Соответствует классу Enumerator из model.py.
    /// </summary>
    public class Enumerator : Base
    {
        /// <summary>Класс-перечисление, которому принадлежит этот элемент (устанавливается после парсинга).</summary>
        [JsonIgnore]
        public Class? Domain { get; set; }

        public override string Id => $"{Domain?.Name}.{Name}";
    }

    /// <summary>
    /// Тип класса в онтологической схеме.
    /// Соответствует enum Type из model.py.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ClassType
    {
        Class,
        Enum,
        Datatype,
        Primitive,
        Compound
    }

    /// <summary>
    /// Представляет класс в онтологической схеме.
    /// Соответствует классу Class из model.py.
    /// </summary>
    public class Class : Base
    {
        [JsonPropertyName("type")]
        public ClassType Type { get; set; } = ClassType.Class;

        /// <summary>Имя базового класса (строковая ссылка из JSON, разрешается в SubClass после парсинга).</summary>
        [JsonPropertyName("sub_class")]
        public string? SubClassName { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, Property> Properties { get; set; } = new();

        [JsonPropertyName("enumerators")]
        public Dictionary<string, Enumerator> Enumerators { get; set; } = new();

        /// <summary>Базовый класс (разрешается из SubClassName после парсинга, не из JSON).</summary>
        [JsonIgnore]
        public Class? SubClass { get; set; }

        /// <summary>SVG-диаграмма класса (заполняется PlantUML, не из JSON).</summary>
        [JsonIgnore]
        public string SvgDiagram { get; set; } = string.Empty;
    }
}
