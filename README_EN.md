# SimpleOntoDoc

> **Beautiful HTML documentation generator for RDF/XML ontologies**

SimpleOntoDoc (internally *RdfsBeautyDoc*) transforms your RDFS/RDF ontology files into a fully searchable, interactive static HTML documentation website — complete with UML class diagrams, cross-linked entities, and a responsive Bootstrap 5 UI.

---

## ✨ Why SimpleOntoDoc?

Ontologies defined in RDF/XML are rich knowledge models, but reading raw XML is painful. SimpleOntoDoc bridges that gap by generating a polished documentation site, similar to what JavaDoc or Sphinx provides for code — but tailored specifically for semantic web schemas.

It was originally created to document the **CIM (Common Information Model)** used in power-system standards (IEC-61970), and works with any RDFS-compliant ontology.

---

## 🚀 Features

| Feature | Description |
|---|---|
| 📄 **HTML site generation** | Produces a complete static HTML website from one or more RDF/XML files |
| 🔍 **Full-text search** | Client-side JSON search index lets users find any class or property instantly |
| 📐 **UML diagrams** | PlantUML-powered SVG class diagrams embedded per page |
| 🏷️ **Stereotype support** | Automatically categorises entities as Class, Enum, DataType, Primitive, UnitSymbol and UnitMultiplier |
| 🔗 **Cross-linking** | Every property domain/range reference links to the corresponding entity page |
| 📱 **Responsive design** | Bootstrap 5 layout works on desktop, tablet and mobile |
| 🐳 **Docker-first deployment** | `publish.bash` builds an nginx Docker image ready to serve |
| 🪟 **Windows + WSL support** | Automatically uses WSL when running Docker commands on Windows |

---

## 📸 Output Structure

Running SimpleOntoDoc on your ontology produces a static website like this:

```
output/
├── index.html                         # Home page with statistics
├── classes/_index.html                # All classes
├── enums/_index.html                  # All enumerations
├── datatypes/_index.html              # All data types
├── primitives/_index.html             # All primitives
├── properties/_index.html             # All properties
├── classes/<ClassName>.html           # One page per class
├── properties/<Domain>.<Prop>.html    # One page per property
└── assets/
    ├── css/site.css
    ├── js/search.js
    └── search-index.json              # Search index
```

---

## 🛠️ Getting Started

### Prerequisites

| Tool | Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **9.0+** | Required to build and run |
| [Docker](https://www.docker.com/get-started) | Any recent version | Required for PlantUML diagram rendering |
| WSL 2 *(Windows only)* | — | Used to run Docker commands on Windows |

### Build from source

```bash
git clone https://github.com/simplepersonru/SimpleOntoDoc.git
cd SimpleOntoDoc

dotnet build RdfsBeautyDoc.csproj
```

### Publish a self-contained binary

```bash
dotnet publish RdfsBeautyDoc.csproj -c Release
```

The binary and all required assets are placed in `bin/Release/net9.0/publish/`.

---

## ⚙️ Configuration

SimpleOntoDoc is configured entirely through **environment variables**.

| Variable | Required | Description |
|---|---|---|
| `RDFSDOC_PATH_TO_RDFS` | ✅ | Path to your input RDF/XML ontology file |
| `RDFSDOC_TITLE` | ✅ | Documentation site title |
| `RDFSDOC_DESCRIPTION` | ✅ | Short description shown on the home page |
| `RDFSDOC_COMMON_NAMESPACE` | ✅ | Namespace prefix used in the ontology (e.g. `cim`) |
| `RDFSDOC_PLANTUML_URL` | ✅ | URL of a running PlantUML server (e.g. `http://localhost:55667`) |
| `RDFSDOC_OUTPUT_PATH` | ✅ | Directory where the generated site will be written |
| `RDFSDOC_USE_NAMESPACE_FOR_PROPERTIES` | ❌ | Set to `true` to include namespace prefix in property names |

---

## 🏃 Usage

### Option 1 — Run with `publish.bash` (recommended for production)

`publish.bash` automates the full pipeline: starts a PlantUML container, generates the site, and packages the result into an nginx Docker image.

```bash
RDFSDOC_PATH_TO_RDFS=/path/to/ontology.xml \
RDFSDOC_TITLE="My RDFS Documentation" \
RDFSDOC_DESCRIPTION="Auto-generated documentation for my ontology" \
RDFSDOC_COMMON_NAMESPACE=cim \
/usr/bin/bash publish.bash
```

This will:
1. Pull and start the `plantuml/plantuml-server` container
2. Run the documentation generator
3. Build an `nginx:alpine` Docker image with the generated site inside
4. Push the image to the configured registry

### Option 2 — Run the binary directly

Start a PlantUML server first (Docker required):

```bash
docker run -d --name plantuml -p 55667:8080 plantuml/plantuml-server
```

Then run the generator:

```bash
RDFSDOC_PATH_TO_RDFS=/path/to/ontology.xml \
RDFSDOC_TITLE="My Ontology" \
RDFSDOC_DESCRIPTION="My ontology description" \
RDFSDOC_COMMON_NAMESPACE=cim \
RDFSDOC_PLANTUML_URL=http://localhost:55667 \
RDFSDOC_OUTPUT_PATH=./output \
dotnet run --project RdfsBeautyDoc.csproj
```

Open `./output/index.html` in your browser to view the result.

### Option 3 — Run via Docker

A pre-built image is available:

```bash
docker run --rm \
  -v ./output:/out \
  --net=host \
  -e RDFSDOC_PATH_TO_RDFS=/out/ontology.xml \
  -e RDFSDOC_TITLE="My Ontology" \
  -e RDFSDOC_DESCRIPTION="My ontology description" \
  -e RDFSDOC_COMMON_NAMESPACE=cim \
  -e RDFSDOC_PLANTUML_URL=http://localhost:55667 \
  -e RDFSDOC_OUTPUT_PATH=/out \
  gitea.simpleperson.ru/admin/rdfs-beauty-doc:latest
```

---

## 📖 How It Works

```
RDF/XML file(s)
      │
      ▼
  XmlParse              ← Parses namespaces, classes, properties,
      │                    enumerations and stereotypes
      ▼
  PlantUML              ← Renders per-class SVG diagrams via a
      │                    PlantUML server running in Docker
      ▼
  SiteGenerator         ← Uses RazorLight (.cshtml) templates to
      │                    produce HTML pages and search-index.json
      ▼
  output/               ← Static website ready to serve with any
                           HTTP server or nginx Docker image
```

---

## 🏗️ Project Structure

```
SimpleOntoDoc/
├── Program.cs          # Entry point and configuration
├── Model.cs            # Domain model: Class, Property, Description, Stereotype
├── XmlParse.cs         # RDF/XML parser
├── SiteGenerator.cs    # HTML generation engine (RazorLight)
├── PlantUML.cs         # PlantUML diagram renderer + Docker manager
├── ViewModel.cs        # View models for Razor templates
├── templates/          # Razor (.cshtml) templates
│   ├── _Layout.cshtml
│   ├── Index.cshtml
│   ├── Class.cshtml
│   ├── ClassList.cshtml
│   ├── Property.cshtml
│   └── PropertyList.cshtml
├── assets/             # Static web assets
│   ├── css/site.css
│   └── js/search.js
├── Dockerfile          # nginx image wrapping generated output
├── publish.bash        # End-to-end build + deploy script
└── RdfsBeautyDoc.csproj
```

---

## 🤝 Contributing

Contributions are welcome! Here is how you can help:

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/my-feature`
3. **Commit** your changes: `git commit -m "Add my feature"`
4. **Push** to your fork: `git push origin feature/my-feature`
5. **Open a Pull Request**

Please follow the existing C# code style (see `.editorconfig`).

---

## 📄 License

No license file is currently included in this repository.  
**All rights reserved** — please contact the author before using, copying, or distributing this software.

---

*Built with ❤️ using [.NET 9](https://dotnet.microsoft.com/), [RazorLight](https://github.com/toddams/RazorLight), [PlantUML](https://plantuml.com/) and [Bootstrap 5](https://getbootstrap.com/).*
