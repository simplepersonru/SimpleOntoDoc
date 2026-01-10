using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RdfsBeautyDoc
{
	internal class XmlParse
	{
		Dictionary<string, Class> _classes = new();
		private readonly Program.Options _options;
		XElement _root;

        public Dictionary<string, Class> Classes { get { return _classes; } }

		public XmlParse(Program.Options options)
		{
			_options = options;

            foreach (var path in options.RdfsPaths)
            {
                Work(path);
            }
		}

        public delegate void Handler(XElement el);
		void handleChildByName(XElement parent, string localName, Handler func)
		{
			foreach (var child in parent.Elements())
			{
				if (child == null)
					continue;
				if (child.Name.LocalName != localName)
					continue;
				func(child);
				break;
			}
		}

		XNamespace? xmlnsOpt(string nsName)
		{
			return _root.GetNamespaceOfPrefix(nsName);
		}

		XNamespace xmlns(string nsName)
		{
			var ns = _root.GetNamespaceOfPrefix(nsName);
			if (ns == null)
				throw new Exception($"Не зарегистрирован namespace {nsName}");
			return ns;
		}

        private string handleSharp(string str)
        {
            if (str.StartsWith("#"))
                return str.Substring(1); // убираем # в начале
            else if (str.Contains("#"))
                return str.Split('#').Last();
            else
                return str;
        }

		// Обновленный getResource
		private string getResource(XElement el, bool optional = false)
		{
			var resourceAttr = el.Attribute(xmlns("rdf").GetName("resource"));
			if (resourceAttr == null && !optional)
				throw new Exception($"Отсутствует rdf:resource у элемента {el.Name}");
            if (resourceAttr == null && optional)
                return string.Empty;


            return handleSharp(resourceAttr!.Value);
		}

		string getId(XElement el)
		{
			var idAttr = el.Attribute(xmlns("rdf").GetName("ID"));
			var aboutAttr = el.Attribute(xmlns("rdf").GetName("about"));

			string? result = null;
			if (idAttr == null && aboutAttr == null)
				throw new Exception($"Отсутствует rdf:ID или rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr != null)
				throw new Exception($"Неоднозначность между rdf:ID и rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr == null)
				result = idAttr.Value;
			else if (idAttr == null && aboutAttr != null)
				result = aboutAttr.Value;

            if (string.IsNullOrEmpty(result))
                throw new Exception($"Пустой идентификатор у элемента {el.Name}");

            return handleSharp(result);
		}

		Class GetOrCreateClass(string name)
		{
			Class? obj;

			if (!_classes.TryGetValue(name, out obj))
			{
				obj = new Class { Name = name };
				_classes[name] = obj;
			}
			return obj;
		}

		private void HandleDescription(XElement el)
		{
			// Определяем тип через <rdf:type rdf:resource="...#Property"/> или ...#Class
			var rdf = xmlns("rdf");

            string id = getId(el);
            string type = string.Empty;
            handleChildByName(el, "type", (child) =>
            {
                type = getResource(child);
            });
            if (string.IsNullOrEmpty(type))
            {
                if (id.Split('.').Length == 2)
                {
                    HandleEnumerator(el);
                }
                else
                {
                    throw new Exception($"Не удалось определить тип Description с id {id}");
                }
            }
            if (type == "Property")
            {
                HandleProperty(el);
                return;
            }

            string stereotype = string.Empty;
            handleChildByName(el, "stereotype", (child) =>
            {
                stereotype = getResource(child, optional:true);
                if (string.IsNullOrEmpty(stereotype))
                    stereotype = child.Value;
                stereotype.ToLower();
            });
            if (stereotype == "enumeration" || type == "Class")
            {
                HandleClass(el);
                return;
            }
            // взяли из значения
            if (stereotype == "enum")
            {
                HandleEnumerator(el);
            }
            if (stereotype == "CIMDatatype")
            {
                HandleClass(el);
            }
        }

		private void HandleClass(XElement el)
		{
			Class cls = GetOrCreateClass(getId(el));
			cls.Namespace = _options.CommonNamespace;

			handleChildByName(el, "label", (child) =>
			{
				cls.Label = child.Value;
			});
			handleChildByName(el, "comment", (child) =>
			{
				cls.Comment = child.Value;
			});
			handleChildByName(el, "stereotype", (child) =>
			{
                string resource = getResource(child);
                string value = child.Value;
                string str = string.IsNullOrEmpty(resource) ? value : resource;
                str = str.ToLower();
                cls.Stereotype = str switch
				{
					"enumeration" => Stereotype.Enum,
					"primitive" => Stereotype.Primitive,
					"datatype" => Stereotype.DataType,
                    "cimdatatype" => Stereotype.DataType,
					_ => Stereotype.Class
				};
			});
			handleChildByName(el, "subClassOf", (child) =>
			{
				cls.SubClass = GetOrCreateClass(getResource(child));
			});
		}

		private void HandleEnumerator(XElement el)
		{
			string id = getId(el);
			string[] splitName = id.Split('.');
			if (splitName.Length != 2)
				return;
			Class? enumClass = GetOrCreateClass(splitName.First());
			if (enumClass == null)
				return;
			enumClass.Stereotype = Stereotype.Enum;
			if (enumClass.Enumerators.ContainsKey(splitName.Last()))
				return;
			Description enumerator = new Description
			{
				Name = splitName.Last(),
				Domain = enumClass,
				Namespace = _options.CommonNamespace
			};
			enumClass.Enumerators.Add(enumerator.Id, enumerator);

			handleChildByName(el, "label", (child) =>
			{
				enumerator.Label = child.Value;
			});
		}

		private void HandleProperty(XElement el)
		{
			Class? domainClass = null;
			handleChildByName(el, "domain", (child) =>
			{
				domainClass = GetOrCreateClass(getResource(child));
			});

			if (domainClass == null)
				return;
			string[] splitName = getId(el).Split('.');
			if (splitName.Length != 2)
				return;
			if (splitName.First() != domainClass.Name)
				return;
			if (domainClass.Properties.ContainsKey(splitName.Last()))
				return;
			Property prop = new Property
			{
				Domain = domainClass,
				Name = splitName.Last(),
			};
			if (_options.UseNamespaceForProperties)
			{
				prop.Namespace = _options.CommonNamespace;
			}
			domainClass.Properties.Add(prop.Name, prop);

			handleChildByName(el, "label", (child) =>
			{
				prop.Label = child.Value;
			});
			handleChildByName(el, "range", (child) =>
			{
				prop.Range = GetOrCreateClass(getResource(child));
			});
			handleChildByName(el, "multiplicity", (child) =>
			{
				prop.Multiplicity = getResource(child).Substring(2); // убираем лишнее "M:"
			});
			handleChildByName(el, "range", (child) =>
			{
				prop.InverseRoleName = getResource(child);
			});
		}

		public Dictionary<string, Class> Work(string path)
		{
            var doc = XDocument.Load(path);
            if (doc == null || doc.Root == null)
                throw new Exception($"Не удалось разобрать xml файла {path}");
            _root = doc.Root;

            foreach (var el in _root.Elements())
			{
				if (el == null)
					continue;
				if (el.Name.LocalName == "Property")
					HandleProperty(el);
				else if (el.Name.LocalName == "Class")
					HandleClass(el);
				else if (el.Name.LocalName == "Description")
					HandleDescription(el);
			}
			return _classes;
		}
	}
}
