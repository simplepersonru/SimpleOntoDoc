using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace RdfsBeautyDoc
{
	internal class XmlParse
	{
		Dictionary<string, Class> _classes = new();
		private readonly Program.Options _options;
		XElement _root;

		public XmlParse(Program.Options options)
		{
			_options = options;

			var doc = XDocument.Load(options.RdfsPath);
			if (doc == null || doc.Root == null)
				throw new Exception($"Не удалось разобрать xml файла {options.RdfsPath}");
			_root = doc.Root;
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

		string getResource(XElement el)
		{
			var resourceAttr = el.Attribute(xmlns("rdf").GetName("resource"));
			if (resourceAttr == null)
				throw new Exception($"Отсутствует rdf:resource у элемента {el.Name}");

			if (resourceAttr.Value.StartsWith("#"))
				return resourceAttr.Value.Substring(1); // убираем # в начале
			else
				return resourceAttr.Value;
		}

		string getId(XElement el)
		{
			var idAttr = el.Attribute(xmlns("rdf").GetName("ID"));
			var aboutAttr = el.Attribute(xmlns("rdf").GetName("about"));

			string result;
			if (idAttr == null && aboutAttr == null)
				throw new Exception($"Отсутствует rdf:ID или rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr != null)
				throw new Exception($"Неоднозначность между rdf:ID и rdf:about у элемента {el.Name}");
			else if (idAttr != null && aboutAttr == null)
				result = idAttr.Value;
			else if (idAttr == null && aboutAttr != null)
				result = aboutAttr.Value;

			throw new UnreachableException();
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

			HandleEnumerator(el);
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
				cls.Stereotype = getResource(child) switch
				{
					"Enumeration" => Stereotype.Enum,
					"Datatype" => Stereotype.DataType,
					"Primitive" => Stereotype.Primitive,
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

		public Dictionary<string, Class> Work()
		{
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
