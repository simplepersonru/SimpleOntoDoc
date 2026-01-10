namespace RdfsBeautyDoc
{
    public abstract class Base
    {
        public string Label { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public virtual string NamespacedId
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return Name;
                else
                    return $"{Namespace}:{Name}";
            }
        }
        public virtual string Id => Name;

    }
    /// <summary>
    /// Представляет свойство (атрибут или связь) класса в онтологической схеме.
    /// Свойство связывает домен (класс-владелец) с диапазоном (типом или другим классом) и содержит дополнительную метаинформацию.
    /// </summary>
    public class Property : Base
    {
        /// <summary>
        /// Класс, которому принадлежит это свойство (домен свойства).
        /// </summary>
        required public Class Domain { get; set; }

        /// <summary>
        /// Ограничение кратности для свойства (например, "1", "0..*", и т.д.).
        /// </summary>
        public string Multiplicity { get; set; } = string.Empty;

        /// <summary>
        /// Имя обратного свойства, если связь двунаправленная.
        /// </summary>
        public string InverseRoleName { get; set; } = string.Empty;

        /// <summary>
        /// Тип свойства (может быть примитивным типом или ссылкой на другой класс).
        /// </summary>
        public Class Range { get; set; }

        /// <summary>
        /// Относительный URL к HTML-документации этого свойства.
        /// </summary>
        public string Href => $"properties/{Domain.Name}.{Name}.html";

        /// <inheritdoc/>
        public override string Id => $"{Domain.Name}.{Name}";

        /// <inheritdoc/>
        public override string NamespacedId
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return Name;
                else
                    return $"{Namespace}:{Name}";
            }
        }

        public string PropertyId => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}:{Name}";
    }

    public class Description : Base
    {
        required public Class Domain { get; set; }

        public override string Id => $"{Domain.Name}.{Name}";

        public override string NamespacedId
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return $"{Domain.Name}.{Name}";
                else
                    return $"{Namespace}:{Domain.Name}.{Name}";
            }
        }
    }


    public enum Stereotype
    {
        Class,
        Enum,
        DataType,
        Primitive,
        UnitSymbol,
        UnitMultiplier,
        All
    }

    public class DataTypeInfo
    { 
        public Class? Value { get; set; }
        public Class? UnitSymbol { get; set; }
        public Class? UnitMultiplier { get; set; }
    }

    public class Class : Base
    {
        public string Comment { get; set; } = string.Empty;
        public string SvgDiagram { get; set; } = string.Empty;
        public Class? SubClass { get; set; }
        public string Href => $"{StereoPath}/{Name}.html";
        public string StereoPath => Stereotype switch
        {
            Stereotype.DataType => "datatypes",
            Stereotype.Enum => "enums",
            Stereotype.Class => "classes",
            Stereotype.Primitive => "primitives",
            Stereotype.All => "entities",
        };

        /// <inheritdoc/>
        public override string NamespacedId
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace)
                    || Stereotype == Stereotype.Primitive
                    || Stereotype == Stereotype.DataType)
                    return Name;
                else
                    return $"{Namespace}:{Name}";
            }
        }

        /// <summary>
        /// Возвращает CSS-класс для бейджа, соответствующего стереотипу класса.
        /// </summary>
        public static string BadgeClassStatic(Stereotype stereotype) => stereotype switch
        {
            Stereotype.Class => "badge-class",
            Stereotype.Enum => "badge-enum",
            Stereotype.Primitive => "badge-primitive",
            Stereotype.DataType => "badge-datatype",
            _ => "badge-secondary"
        };

        /// <summary>
        /// CSS-класс для бейджа текущего стереотипа класса.
        /// </summary>
        public string BadgeClass => BadgeClassStatic(Stereotype);

        /// <summary>
        /// Стереотип класса (например, Class, Enum, Primitive, DataType).
        /// </summary>
        public Stereotype Stereotype { get; set; } = Stereotype.Class;

        /// <summary>
        /// Свойства (атрибуты и связи), определённые для этого класса.
        /// </summary>
        public Dictionary<string, Property> Properties { get; set; } = new Dictionary<string, Property>();

        /// <summary>
        /// Значения перечисления для этого класса (если класс — перечисление).
        /// </summary>
        public Dictionary<string, Description> Enumerators { get; set; } = new Dictionary<string, Description>();
    }
}
