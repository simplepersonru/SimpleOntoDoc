# SimpleOntoDoc

> **Генератор документации по онтологическим схемам (HTML и Markdown)**

SimpleOntoDoc принимает онтологическую схему в собственном формате (JSON/HJSON) и генерирует статическую документацию:
- **HTML-сайт** с поиском и навигацией;
- **Markdown-пакет** (`Readme.md` + страницы сущностей).

---

## 🔀 Входной формат

SimpleOntoDoc ожидает **один файл схемы** в формате JSON или HJSON. Структура описана в [`model.py`](./model.py).

Корневой объект — массив `Class`.

Ключевые моменты текущего формата:
- `properties` — **список** объектов `Property`;
- `enumerators` — **список** объектов `Enumerator`;
- `relations` — список ручных связей между классами (`left`, `right`, `relation_line`);
- ссылки (`sub_class`, `range`, `left`, `right`) задаются строковыми именами классов и резолвятся парсером.

Минимальный пример:

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
      { "name": "Active", "namespace": "demo", "description": "Активен" }
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

Поддерживаемые `type`: `Class`, `Enum`, `Datatype`, `Primitive`, `Compound`.

Готовый минимальный разнообразный пример есть в [`Example2/schema.hjson`](./Example2/schema.hjson).

---

## 🚀 Возможности

| Возможность | Описание |
|---|---|
| 📄 **HTML-генерация** | Полноценный статический сайт с навигацией и поиском |
| 📝 **Markdown-генерация** | Генерация `Readme.md` + `entities/*.md` |
| 🧾 **JSON/HJSON input** | Поддержка `.json` и `.hjson` входных файлов |
| 📐 **UML/PlantUML** | Диаграммы классов в HTML или PlantUML-блоки в Markdown-режиме |
| 🔗 **Связи и ссылки** | Резолвинг `sub_class`/`range`, поддержка `relations` и `relation_line` |
| 🔍 **Поиск** | Клиентский search-index (`assets/search-index.json`) |

---

## 📸 Структура выхода

### HTML-режим (`SIMPLEDOC_MARKDOWN_RENDER=false`)

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

### Markdown-режим (`SIMPLEDOC_MARKDOWN_RENDER=true`)

```
output/
├── Readme.md
└── entities/
    └── <ClassName>.md
```

---

## 🛠️ Начало работы

### Требования

| Инструмент | Версия | Примечание |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0+** | Сборка и запуск |
| [Docker](https://www.docker.com/get-started) | опционально | Нужен для полноценного PlantUML-рендеринга в HTML-режиме |
| WSL 2 *(Windows)* | опционально | Для запуска docker-команд из Windows |

### Сборка

```bash
dotnet build SimpleOntoDoc.csproj
```

### Публикация

```bash
dotnet publish SimpleOntoDoc.csproj -c Release
```

Публикация попадёт в `bin/Release/net10.0/publish/`.

---

## ⚙️ Конфигурация (ENV)

| Переменная | Обязательна | Описание |
|---|---|---|
| `SIMPLEDOC_INPUT_PATH` | ✅ | Путь к входному `.json` или `.hjson` |
| `SIMPLEDOC_OUTPUT_PATH` | ✅ | Путь к выходной директории |
| `SIMPLEDOC_TITLE` | ✅ | Заголовок документации |
| `SIMPLEDOC_DESCRIPTION` | ✅ | Описание на главной странице |
| `SIMPLEDOC_BASE_PATH` | ❌ | Базовый путь сайта (например, `/docs`) |
| `SIMPLEDOC_MARKDOWN_RENDER` | ❌ | `true` → Markdown-режим |
| `SIMPLEDOC_PLANTUML_SKIP` | ❌ | `true` → пропустить шаг PlantUML |
| `SIMPLEDOC_PLANTUML_URL` | ❌ | URL PlantUML-сервера (если не задан, приложение пытается поднять Docker-контейнер в HTML-режиме) |

---

## 🏃 Использование

### HTML-режим

```bash
SIMPLEDOC_INPUT_PATH=./Example/ontology.json \
SIMPLEDOC_OUTPUT_PATH=./output \
SIMPLEDOC_TITLE="My Ontology" \
SIMPLEDOC_DESCRIPTION="Generated ontology docs" \
SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
dotnet run --no-launch-profile --project SimpleOntoDoc.csproj
```

### Markdown-режим

```bash
SIMPLEDOC_INPUT_PATH=./Example2/schema.hjson \
SIMPLEDOC_OUTPUT_PATH=./Example2/output \
SIMPLEDOC_TITLE="Example2 Markdown Ontology" \
SIMPLEDOC_DESCRIPTION="Minimal diverse HJSON example" \
SIMPLEDOC_MARKDOWN_RENDER=true \
SIMPLEDOC_PLANTUML_SKIP=true \
dotnet run --no-launch-profile --project SimpleOntoDoc.csproj
```

Для `Example2` также есть готовый скрипт: [`Example2/example_markdown.bash`](./Example2/example_markdown.bash).

---

## 🧱 Структура проекта

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

*Сделано с ❤️ на [.NET 10](https://dotnet.microsoft.com/), [RazorLight](https://github.com/toddams/RazorLight) и [PlantUML](https://plantuml.com/).*