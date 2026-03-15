# SimpleOntoDoc

> **Красивый генератор HTML-документации для онтологических схем**

SimpleOntoDoc принимает онтологическую схему в собственном JSON-формате и генерирует полнофункциональный статический HTML-сайт документации с поддержкой поиска — включая UML-диаграммы классов, перекрёстные ссылки между сущностями и адаптивный интерфейс на Bootstrap 5.

---

## 🔀 Входной формат

SimpleOntoDoc ожидает на входе **один JSON-файл**, структура которого описана в [`model.py`](./model.py) в виде Python dataclass-ов. Файл представляет собой JSON-массив объектов классов (`Class`):

```json
[
  {
    "description": "Базовый класс...",
    "namespace": "cim",
    "name": "IdentifiedObject",
    "type": "Class",
    "sub_class": null,
    "properties": {
      "mRID": {
        "description": "UUID объекта.",
        "namespace": "cim",
        "name": "mRID",
        "range": "String",
        "optional": true
      }
    }
  },
  {
    "description": "Тип заземления.",
    "namespace": "rf",
    "name": "ShieldGroundingKind",
    "type": "Enum",
    "sub_class": null,
    "enumerators": {
      "none": { "description": "Нет заземления.", "namespace": "rf", "name": "none" }
    }
  }
]
```

Поддерживаемые значения поля `type`: `Class`, `Enum`, `Datatype`, `Primitive`, `Compound`.

Ссылки между классами (`sub_class`, `range`) задаются строками с именем класса, а не вложенными объектами — для избежания рекурсии. `JsonParse.cs` автоматически разрешает эти ссылки через механизм `GetOrCreate`.

### Как получить входной файл?

Онтологические схемы существуют в разных форматах (RDF/XML, OWL, XMI, EA UML, ...). В репозитории содержится [`model.py`](./model.py) с описанием структуры внутреннего формата SimpleOntoDoc. Для преобразования из конкретного формата в этот JSON необходимо написать скрипт-конвертер (Python или другой язык). SimpleOntoDoc намеренно не привязан ни к одному конкретному источнику онтологий.

---

## ✨ Зачем SimpleOntoDoc?

Онтологии содержат богатые модели знаний, но читать их исходники — мучение. SimpleOntoDoc устраняет этот разрыв, генерируя аккуратный сайт документации — по аналогии с тем, что JavaDoc или Sphinx делают для кода, — но адаптированный специально для онтологических схем.

---

## 🚀 Возможности

| Возможность | Описание |
|---|---|
| 📄 **Генерация HTML-сайта** | Создаёт полноценный статический HTML-сайт из одного JSON-файла |
| 🔍 **Полнотекстовый поиск** | Клиентский JSON-индекс позволяет мгновенно найти любой класс или свойство |
| 📐 **UML-диаграммы** | SVG-диаграммы классов на основе PlantUML, встроенные на каждую страницу |
| 🏷️ **Поддержка типов** | Автоматически классифицирует сущности: Class, Enum, Datatype, Primitive, Compound |
| 🔗 **Перекрёстные ссылки** | Каждая ссылка на домен/диапазон свойства ведёт на страницу соответствующей сущности |
| 📱 **Адаптивный дизайн** | Вёрстка на Bootstrap 5 работает на десктопе, планшете и мобильном устройстве |
| 🐳 **Docker-ориентированное развёртывание** | `publish.bash` собирает образ Docker с nginx, готовый к запуску |
| 🪟 **Поддержка Windows + WSL** | Автоматически использует WSL для команд Docker в Windows |
| 🧹 **Чистая генерация** | Выходная директория очищается перед каждой генерацией — нет «зависших» файлов |

---

## 📸 Структура выходных файлов

```
output/
├── index.html                         # Главная страница со статистикой
├── classes/_index.html                # Все классы
├── enums/_index.html                  # Все перечисления
├── datatypes/_index.html              # Все типы данных
├── primitives/_index.html             # Все примитивы
├── compounds/_index.html              # Все составные типы
├── properties/_index.html             # Все свойства
├── classes/<ClassName>.html           # Отдельная страница на каждый класс
├── properties/<Domain>.<Prop>.html    # Отдельная страница на каждое свойство
└── assets/
    ├── css/site.css
    ├── js/search.js
    └── search-index.json              # Поисковый индекс
```

---

## 🛠️ Начало работы

### Требования

| Инструмент | Версия | Примечание |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **9.0+** | Необходим для сборки и запуска |
| [Docker](https://www.docker.com/get-started) | Любая актуальная версия | Необходим для рендеринга диаграмм PlantUML |
| WSL 2 *(только Windows)* | — | Используется для команд Docker в Windows |

### Сборка из исходников

```bash
git clone https://github.com/simplepersonru/SimpleOntoDoc.git
cd SimpleOntoDoc

dotnet build SimpleOntoDoc.csproj
```

### Публикация самодостаточного бинарника

```bash
dotnet publish SimpleOntoDoc.csproj -c Release
```

Бинарник и все необходимые ресурсы будут помещены в `bin/Release/net9.0/publish/`.

---

## ⚙️ Конфигурация

SimpleOntoDoc настраивается полностью через **переменные окружения**.

| Переменная | Обязательна | Описание |
|---|---|---|
| `SIMPLEDOC_INPUT_PATH` | ✅ | Путь к входному JSON-файлу онтологии |
| `SIMPLEDOC_TITLE` | ✅ | Заголовок сайта документации |
| `SIMPLEDOC_DESCRIPTION` | ✅ | Краткое описание, отображаемое на главной странице |
| `SIMPLEDOC_PLANTUML_URL` | ✅ | URL работающего сервера PlantUML (например, `http://localhost:55667`) |
| `SIMPLEDOC_OUTPUT_PATH` | ✅ | Директория, в которую будет записан сгенерированный сайт |

---

## 🏃 Использование

### Вариант 1 — Запуск через `publish.bash` (рекомендуется для продакшена)

`publish.bash` автоматизирует весь процесс: запускает контейнер PlantUML, генерирует сайт и упаковывает результат в образ Docker с nginx.

```bash
SIMPLEDOC_INPUT_PATH=/path/to/ontology.json \
SIMPLEDOC_TITLE="Моя документация" \
SIMPLEDOC_DESCRIPTION="Автоматически сгенерированная документация" \
/usr/bin/bash publish.bash
```

### Вариант 2 — Запуск бинарника напрямую

Сначала запустите сервер PlantUML (требуется Docker):

```bash
docker run -d --name plantuml -p 55667:8080 plantuml/plantuml-server
```

Затем запустите генератор:

```bash
SIMPLEDOC_INPUT_PATH=/path/to/ontology.json \
SIMPLEDOC_TITLE="Моя онтология" \
SIMPLEDOC_DESCRIPTION="Описание моей онтологии" \
SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
SIMPLEDOC_OUTPUT_PATH=./output \
dotnet run --project SimpleOntoDoc.csproj
```

Откройте `./output/index.html` в браузере для просмотра результата.

### Вариант 3 — Запуск через Docker

```bash
docker run --rm \
  -v ./output:/out \
  --net=host \
  -e SIMPLEDOC_INPUT_PATH=/out/ontology.json \
  -e SIMPLEDOC_TITLE="Моя онтология" \
  -e SIMPLEDOC_DESCRIPTION="Описание моей онтологии" \
  -e SIMPLEDOC_PLANTUML_URL=http://localhost:55667 \
  -e SIMPLEDOC_OUTPUT_PATH=/out \
  gitea.simpleperson.ru/admin/simple-onto-doc:latest
```

---

## 📖 Как это работает

```
JSON файл (формат model.py)
      │
      ▼
  JsonParse             ← Разбирает JSON-массив классов, разрешает строковые
      │                    ссылки (sub_class, range) в объекты через GetOrCreate
      ▼
  PlantUML              ← Рендерит SVG-диаграммы для каждого класса
      │                    через сервер PlantUML в Docker
      ▼
  SiteGenerator         ← Использует шаблоны RazorLight (.cshtml) для
      │                    генерации HTML-страниц и search-index.json
      ▼
  output/               ← Статический сайт, готовый к публикации через
                           любой HTTP-сервер или образ Docker с nginx
```

---

## 🏗️ Структура проекта

```
SimpleOntoDoc/
├── Program.cs          # Точка входа и конфигурация (чтение ENV)
├── Model.cs            # Доменная модель: Class, Property, Enumerator, ClassType
│                         (поля с [JsonPropertyName] соответствуют model.py)
├── JsonParse.cs        # Парсер JSON-формата model.py
├── SiteGenerator.cs    # Движок генерации HTML (RazorLight)
├── PlantUML.cs         # Рендерер диаграмм PlantUML + управление Docker
├── ViewModel.cs        # View-модели + расширения ClassExtensions/PropertyExtensions
├── model.py            # Python dataclass-описание входного формата
├── templates/          # Razor (.cshtml) шаблоны
│   ├── _Layout.cshtml
│   ├── Index.cshtml
│   ├── Class.cshtml
│   ├── ClassList.cshtml
│   ├── Property.cshtml
│   └── PropertyList.cshtml
├── assets/             # Статические веб-ресурсы
│   ├── css/site.css
│   └── js/search.js
├── Dockerfile          # nginx-образ, оборачивающий сгенерированный сайт
├── publish.bash        # Скрипт сборки и деплоя
└── SimpleOntoDoc.csproj
```

---

## 🤝 Участие в разработке

Мы рады вашему участию! Вот как вы можете помочь:

1. **Форкните** репозиторий
2. **Создайте** ветку для своей задачи: `git checkout -b feature/my-feature`
3. **Зафиксируйте** изменения: `git commit -m "Добавить мою функцию"`
4. **Отправьте** ветку в ваш форк: `git push origin feature/my-feature`
5. **Откройте Pull Request**

Пожалуйста, соблюдайте существующий стиль кода C# (см. `.editorconfig`).

---

## 📄 Лицензия

Файл лицензии в репозитории отсутствует.  
**Все права защищены** — пожалуйста, свяжитесь с автором, прежде чем использовать, копировать или распространять это программное обеспечение.

---

*Создано с ❤️ с использованием [.NET 9](https://dotnet.microsoft.com/), [RazorLight](https://github.com/toddams/RazorLight), [PlantUML](https://plantuml.com/) и [Bootstrap 5](https://getbootstrap.com/).*
