# SimpleOntoDoc

> **Ontology documentation generator (HTML and Markdown)**

SimpleOntoDoc consumes an ontology schema in its internal format (JSON/HJSON) and generates static documentation:
- **HTML website** with navigation and search;
- **Markdown package** (`Readme.md` + entity pages).

---

## 🔀 Input format

SimpleOntoDoc expects a single schema file in JSON or HJSON format. The structure is described in [`model.py`](./model.py).

Root object: array of `Class` objects.

Current format highlights:
- `properties` is a **list** of `Property` objects;
- `enumerators` is a **list** of `Enumerator` objects;
- `relations` is a list of explicit class relations (`left`, `right`, `relation_line`);
- references (`sub_class`, `range`, `left`, `right`) are string class names and are resolved by the parser.

Minimal example:

```json
[
  {
    "name": "String",
    "namespace": "demo",
    "type": "Primitive",
    "sub_class": null,
    "relations": [],
    "properties": [],
    "enumerators": []
  },
  {
    "name": "StatusKind",
    "namespace": "demo",
    "type": "Enum",
    "sub_class": null,
    "relations": [],
    "properties": [],
    "enumerators": [
      { "name": "Active", "namespace": "demo", "description": "Active" }
    ]
  },
  {
    "name": "Device",
    "namespace": "demo",
    "type": "Class",
    "sub_class": null,
    "relations": [],
    "properties": [
      {
        "name": "status",
        "namespace": "demo",
        "range": "StatusKind",
        "optional": false,
        "multiplicity": "1"
      }
    ],
    "enumerators": []
  }
]
```

Supported `type` values: `Class`, `Enum`, `Datatype`, `Primitive`, `Compound`.

A ready-to-run minimal diverse sample is available at [`Example2/schema.hjson`](./Example2/schema.hjson).

---

## 🚀 Features

| Feature | Description |
|---|---|
| 📄 **HTML generation** | Full static site with pages, navigation and search |
| 📝 **Markdown generation** | Generates `Readme.md` + `entities/*.md` |
| 🧾 **JSON/HJSON input** | Supports both `.json` and `.hjson` schema files |
| 📐 **UML/PlantUML** | Class diagrams in HTML or PlantUML blocks in Markdown mode |
| 🔗 **Links and relations** | Resolves `sub_class`/`range`, supports `relations` and `relation_line` |
| 🔍 **Search index** | Client-side search via `assets/search-index.json` |

---

## 📸 Output structure

### HTML mode (`SIMPLEDOC_MARKDOWN_RENDER=false`)

```
output/
├── index.html
├── entities.html
├── classes/_index.html
├── enums/_index.html
├── datatypes/_index.html
├── primitives/_index.html
├── compounds/_index.html
├── properties/_index.html
├── classes/<ClassName>.html
├── properties/<Domain>.<Prop>.html
└── assets/
    ├── css/site.css
    ├── js/search.js
    └── search-index.json
```

### Markdown mode (`SIMPLEDOC_MARKDOWN_RENDER=true`)

```
output/
├── Readme.md
└── entities/
    └── <ClassName>.md
```

---

## 🛠️ Getting started

### Prerequisites

| Tool | Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0+** | Build and run |
| [Docker](https://www.docker.com/get-started) | optional | Needed for full PlantUML rendering in HTML mode |
| WSL 2 *(Windows)* | optional | Used for Docker commands on Windows |

### Build

```bash
dotnet build SimpleOntoDoc.csproj
```

### Publish

```bash
dotnet publish SimpleOntoDoc.csproj -c Release
```

Publish output path: `bin/Release/net10.0/publish/`.

---

## ⚙️ Configuration (ENV)

| Variable | Required | Description |
|---|---|---|
| `SIMPLEDOC_INPUT_PATH` | ✅ | Input `.json` or `.hjson` path |
| `SIMPLEDOC_OUTPUT_PATH` | ✅ | Output directory |
| `SIMPLEDOC_TITLE` | ✅ | Documentation title |
| `SIMPLEDOC_DESCRIPTION` | ✅ | Home page description |
| `SIMPLEDOC_BASE_PATH` | ❌ | Site base path (for example `/docs`) |
| `SIMPLEDOC_MARKDOWN_RENDER` | ❌ | `true` => Markdown mode |
| `SIMPLEDOC_PLANTUML_SKIP` | ❌ | `true` => skip PlantUML step |
| `SIMPLEDOC_PLANTUML_URL` | ❌ | PlantUML server URL (if omitted, app tries to auto-start Docker PlantUML in HTML mode) |

---

## 🏃 Usage

### HTML mode

```bash
SIMPLEDOC_INPUT_PATH=./Example/ontology.json \
SIMPLEDOC_OUTPUT_PATH=./output \
SIMPLEDOC_TITLE="My Ontology" \
SIMPLEDOC_DESCRIPTION="Generated ontology docs" \
SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
dotnet run --no-launch-profile --project SimpleOntoDoc.csproj
```

### Markdown mode

```bash
SIMPLEDOC_INPUT_PATH=./Example2/schema.hjson \
SIMPLEDOC_OUTPUT_PATH=./Example2/output \
SIMPLEDOC_TITLE="Example2 Markdown Ontology" \
SIMPLEDOC_DESCRIPTION="Minimal diverse HJSON example" \
SIMPLEDOC_MARKDOWN_RENDER=true \
SIMPLEDOC_PLANTUML_SKIP=true \
dotnet run --no-launch-profile --project SimpleOntoDoc.csproj
```

For `Example2`, you can also run [`Example2/example_markdown.bash`](./Example2/example_markdown.bash).

---

## 🧱 Project structure

```
SimpleOntoDoc/
├── Program.cs
├── Model.cs
├── JsonParse.cs
├── SiteGenerator.cs
├── MarkdownGenerator.cs
├── PlantUML.cs
├── ViewModel.cs
├── model.py
├── templates/
│   ├── Index.cshtml
│   ├── Class.cshtml
│   ├── ClassList.cshtml
│   ├── Property.cshtml
│   ├── PropertyList.cshtml
│   ├── Index_md.cshtml
│   └── Class_md.cshtml
├── Example/
├── Example2/
└── SimpleOntoDoc.Tests/
```

*Built with ❤️ using [.NET 10](https://dotnet.microsoft.com/), [RazorLight](https://github.com/toddams/RazorLight) and [PlantUML](https://plantuml.com/).*