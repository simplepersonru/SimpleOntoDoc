# SimpleOntoDoc

> **Beautiful HTML documentation generator for ontology schemas**

SimpleOntoDoc takes an ontology schema in its own JSON format and produces a fully searchable, interactive static HTML documentation website — complete with UML class diagrams, cross-linked entities, and a responsive Bootstrap 5 UI.

---

## 🔀 Input Format

SimpleOntoDoc expects **a single JSON file** whose structure is defined in [`model.py`](./model.py) as Python dataclasses. The file is a JSON array of `Class` objects:

```json
[
  {
    "description": "Base class...",
    "namespace": "cim",
    "name": "IdentifiedObject",
    "type": "Class",
    "sub_class": null,
    "properties": {
      "mRID": {
        "description": "UUID of the object.",
        "namespace": "cim",
        "name": "mRID",
        "range": "String",
        "optional": true
      }
    }
  },
  {
    "description": "Grounding type.",
    "namespace": "rf",
    "name": "ShieldGroundingKind",
    "type": "Enum",
    "sub_class": null,
    "enumerators": {
      "none": { "description": "No grounding.", "namespace": "rf", "name": "none" }
    }
  }
]
```

Supported `type` values: `Class`, `Enum`, `Datatype`, `Primitive`, `Compound`.

Cross-references (`sub_class`, `range`) are stored as class name strings rather than nested objects (to avoid recursion). `JsonParse.cs` resolves them automatically via a `GetOrCreate` mechanism.

### How to obtain the input file?

Ontology schemas exist in many formats (RDF/XML, OWL, XMI, EA UML, …). The repository contains [`model.py`](./model.py) describing the internal SimpleOntoDoc format. To convert from a specific source format you need a converter script (Python or any other language). SimpleOntoDoc is intentionally decoupled from any particular ontology source.

---

## ✨ Why SimpleOntoDoc?

Ontologies are rich knowledge models, but reading their source files is painful. SimpleOntoDoc bridges that gap by generating a polished documentation site, similar to what JavaDoc or Sphinx provides for code — but tailored for ontology schemas.

---

## 🚀 Features

| Feature | Description |
|---|---|
| 📄 **HTML site generation** | Produces a complete static HTML website from a single JSON file |
| 🔍 **Full-text search** | Client-side JSON search index for instant lookup of any class or property |
| 📐 **UML diagrams** | PlantUML-powered SVG class diagrams embedded per page |
| 🏷️ **Type support** | Automatically categorises entities as Class, Enum, Datatype, Primitive, Compound |
| 🔗 **Cross-linking** | Every property domain/range reference links to the corresponding entity page |
| 📱 **Responsive design** | Bootstrap 5 layout works on desktop, tablet and mobile |
| 🐳 **Docker-first deployment** | `publish.bash` builds an nginx Docker image ready to serve |
| 🪟 **Windows + WSL support** | Automatically uses WSL when running Docker commands on Windows |
| 🧹 **Clean generation** | Output directory is wiped before each run — no stale leftover files |

---

## 📸 Output Structure

```
output/
├── index.html                         # Home page with statistics
├── classes/_index.html                # All classes
├── enums/_index.html                  # All enumerations
├── datatypes/_index.html              # All data types
├── primitives/_index.html             # All primitives
├── compounds/_index.html              # All compound types
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

dotnet build SimpleOntoDoc.csproj
```

### Publish a self-contained binary

```bash
dotnet publish SimpleOntoDoc.csproj -c Release
```

The binary and all required assets are placed in `bin/Release/net9.0/publish/`.

---

## ⚙️ Configuration

SimpleOntoDoc is configured entirely through **environment variables**.

| Variable | Required | Description |
|---|---|---|
| `SIMPLEDOC_INPUT_PATH` | ✅ | Path to the input JSON ontology file |
| `SIMPLEDOC_TITLE` | ✅ | Documentation site title |
| `SIMPLEDOC_DESCRIPTION` | ✅ | Short description shown on the home page |
| `SIMPLEDOC_PLANTUML_URL` | ✅ | URL of a running PlantUML server (e.g. `http://localhost:55667`) |
| `SIMPLEDOC_OUTPUT_PATH` | ✅ | Directory where the generated site will be written |

---

## 🏃 Usage

### Option 1 — Run with `publish.bash` (recommended for production)

```bash
SIMPLEDOC_INPUT_PATH=/path/to/ontology.json \
SIMPLEDOC_TITLE="My Documentation" \
SIMPLEDOC_DESCRIPTION="Auto-generated ontology documentation" \
/usr/bin/bash publish.bash
```

### Option 2 — Run the binary directly

Start a PlantUML server first (Docker required):

```bash
docker run -d --name plantuml -p 55667:8080 plantuml/plantuml-server
```

Then run the generator:

```bash
SIMPLEDOC_INPUT_PATH=/path/to/ontology.json \
SIMPLEDOC_TITLE="My Ontology" \
SIMPLEDOC_DESCRIPTION="My ontology description" \
SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
SIMPLEDOC_OUTPUT_PATH=./output \
dotnet run --project SimpleOntoDoc.csproj
```

Open `./output/index.html` in your browser to view the result.

### Option 3 — Run via Docker

```bash
docker run --rm \
  -v ./output:/out \
  --net=host \
  -e SIMPLEDOC_INPUT_PATH=/out/ontology.json \
  -e SIMPLEDOC_TITLE="My Ontology" \
  -e SIMPLEDOC_DESCRIPTION="My ontology description" \
  -e SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
  -e SIMPLEDOC_OUTPUT_PATH=/out \
  gitea.simpleperson.ru/admin/simple-onto-doc:latest
```

---

## 📖 How It Works

```
JSON file (model.py format)
      │
      ▼
  JsonParse             ← Parses JSON array of classes, resolves string
      │                    references (sub_class, range) via GetOrCreate
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
├── Program.cs          # Entry point and configuration (reads ENV vars)
├── Model.cs            # Domain model: Class, Property, Enumerator, ClassType
│                         (fields with [JsonPropertyName] mirror model.py)
├── JsonParse.cs        # JSON parser for the model.py format
├── SiteGenerator.cs    # HTML generation engine (RazorLight)
├── PlantUML.cs         # PlantUML diagram renderer + Docker manager
├── ViewModel.cs        # View models + ClassExtensions/PropertyExtensions helpers
├── model.py            # Python dataclass description of the input format
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
└── SimpleOntoDoc.csproj
```

*Built with ❤️ using [.NET 9](https://dotnet.microsoft.com/), [RazorLight](https://github.com/toddams/RazorLight), [PlantUML](https://plantuml.com/) and [Bootstrap 5](https://getbootstrap.com/).*
