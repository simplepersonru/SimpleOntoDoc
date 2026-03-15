using System.IO;
using Xunit;

namespace SimpleOntoDoc.Tests
{
    /// <summary>
    /// Модульные тесты для JsonParse.
    /// Используют онтологию Animals (Data/animals.json), покрывающую все типы сущностей,
    /// наследование, свойства, перечисления и все поля формата.
    /// </summary>
    public class JsonParseTests
    {
        private static readonly string AnimalsJsonPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "animals.json");

        private static Dictionary<string, Class> ParseAnimals()
        {
            var options = new Program.Options
            {
                InputJsonPath = AnimalsJsonPath,
                OutputPath = Path.GetTempPath(),
                DocTitle = "Test",
                DocDescription = "Test",
                SkipPlantUml = true,
            };
            return new JsonParse(options).Classes;
        }

        [Fact]
        public void Parse_AnimalsOntology_LoadsCorrectTotalCount()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            // 5 Classes + 1 Enum + 3 Primitives + 1 Datatype + 1 Compound = 11
            Assert.Equal(11, classes.Count);
        }

        [Fact]
        public void Parse_AnimalsOntology_ClassTypeCounts()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Assert.Equal(5, classes.Values.Count(c => c.Type == ClassType.Class));
            Assert.Equal(1, classes.Values.Count(c => c.Type == ClassType.Enum));
            Assert.Equal(3, classes.Values.Count(c => c.Type == ClassType.Primitive));
            Assert.Equal(1, classes.Values.Count(c => c.Type == ClassType.Datatype));
            Assert.Equal(1, classes.Values.Count(c => c.Type == ClassType.Compound));
        }

        [Fact]
        public void Parse_AnimalsOntology_AllExpectedClassesPresent()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            string[] expected = ["LivingBeing", "Animal", "Mammal", "Dog", "Person",
                                  "DietKind", "String", "Integer", "Boolean", "Date", "GeoPosition"];
            foreach (string name in expected)
                Assert.True(classes.ContainsKey(name), $"Ожидаемый класс '{name}' не найден.");
        }

        [Fact]
        public void Parse_AnimalsOntology_InheritanceChainResolved()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            // Dog → Mammal → Animal → LivingBeing
            Assert.Equal("Mammal", classes["Dog"].SubClass?.Name);
            Assert.Equal("Animal", classes["Mammal"].SubClass?.Name);
            Assert.Equal("LivingBeing", classes["Animal"].SubClass?.Name);
            Assert.Null(classes["LivingBeing"].SubClass);

            // Person → LivingBeing
            Assert.Equal("LivingBeing", classes["Person"].SubClass?.Name);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyDomainResolved()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property ageProp = classes["Animal"].Properties["age"];
            Assert.Equal("Animal", ageProp.Domain.Name);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyRangeResolvedToPrimitive()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property nameProp = classes["LivingBeing"].Properties["name"];
            Assert.Equal("String", nameProp.Range.Name);
            Assert.Equal(ClassType.Primitive, nameProp.Range.Type);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyRangeResolvedToEnum()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property dietProp = classes["Animal"].Properties["diet"];
            Assert.Equal("DietKind", dietProp.Range.Name);
            Assert.Equal(ClassType.Enum, dietProp.Range.Type);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyRangeResolvedToClass()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property ownerProp = classes["Dog"].Properties["owner"];
            Assert.Equal("Person", ownerProp.Range.Name);
            Assert.Equal(ClassType.Class, ownerProp.Range.Type);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyRangeResolvedToCompound()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property habitatProp = classes["Person"].Properties["habitat"];
            Assert.Equal("GeoPosition", habitatProp.Range.Name);
            Assert.Equal(ClassType.Compound, habitatProp.Range.Type);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyRangeResolvedToDatatype()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property birthDateProp = classes["Person"].Properties["birthDate"];
            Assert.Equal("Date", birthDateProp.Range.Name);
            Assert.Equal(ClassType.Datatype, birthDateProp.Range.Type);
        }

        [Fact]
        public void Parse_AnimalsOntology_OptionalFlagParsed()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            // name в LivingBeing — обязательное (optional: false)
            Assert.False(classes["LivingBeing"].Properties["name"].Optional);

            // age в Animal — необязательное (optional: true)
            Assert.True(classes["Animal"].Properties["age"].Optional);

            // diet в Animal — обязательное (optional: false)
            Assert.False(classes["Animal"].Properties["diet"].Optional);
        }

        [Fact]
        public void Parse_AnimalsOntology_MultiplicityParsed()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Assert.Equal("0..1", classes["Animal"].Properties["age"].Multiplicity);
            Assert.Equal("0..*", classes["Mammal"].Properties["furColor"].Multiplicity);
            Assert.Equal("0..*", classes["Person"].Properties["pets"].Multiplicity);
            Assert.Null(classes["Dog"].Properties["breed"].Multiplicity);
        }

        [Fact]
        public void Parse_AnimalsOntology_InverseRoleNameParsed()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Assert.Equal("pets", classes["Dog"].Properties["owner"].InverseRoleName);
            Assert.Equal("owner", classes["Person"].Properties["pets"].InverseRoleName);
            Assert.Null(classes["Dog"].Properties["breed"].InverseRoleName);
        }

        [Fact]
        public void Parse_AnimalsOntology_EnumeratorsHaveCorrectDomain()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Class dietKind = classes["DietKind"];
            Assert.Equal(3, dietKind.Enumerators.Count);

            foreach (Enumerator enumerator in dietKind.Enumerators.Values)
                Assert.Equal("DietKind", enumerator.Domain.Name);
        }

        [Fact]
        public void Parse_AnimalsOntology_EnumeratorNamesCorrect()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Class dietKind = classes["DietKind"];
            Assert.True(dietKind.Enumerators.ContainsKey("herbivore"));
            Assert.True(dietKind.Enumerators.ContainsKey("carnivore"));
            Assert.True(dietKind.Enumerators.ContainsKey("omnivore"));
        }

        [Fact]
        public void Parse_AnimalsOntology_EnumeratorIdFormatCorrect()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Enumerator herbivore = classes["DietKind"].Enumerators["herbivore"];
            Assert.Equal("DietKind.herbivore", herbivore.Id);
        }

        [Fact]
        public void Parse_AnimalsOntology_DescriptionsLoaded()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Assert.False(string.IsNullOrEmpty(classes["LivingBeing"].Description));
            Assert.False(string.IsNullOrEmpty(classes["DietKind"].Description));
            Assert.False(string.IsNullOrEmpty(classes["Dog"].Properties["breed"].Description));
        }

        [Fact]
        public void Parse_AnimalsOntology_NamespacesLoaded()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            // Все классы в онтологии имеют namespace "animal"
            foreach (Class cls in classes.Values)
                Assert.Equal("animal", cls.Namespace);
        }

        [Fact]
        public void Parse_AnimalsOntology_PropertyIdFormatCorrect()
        {
            Dictionary<string, Class> classes = ParseAnimals();

            Property ageProp = classes["Animal"].Properties["age"];
            Assert.Equal("Animal.age", ageProp.Id);
        }
    }
}
