# SimpleOntoDoc

> **Красивый генератор HTML-документации для RDF/XML-онтологий**

SimpleOntoDoc (внутреннее название *RdfsBeautyDoc*) преобразует файлы RDFS/RDF-онтологий в полнофункциональный статический HTML-сайт документации с поддержкой поиска — включая UML-диаграммы классов, перекрёстные ссылки между сущностями и адаптивный интерфейс на Bootstrap 5.

---

## ✨ Зачем SimpleOntoDoc?

Онтологии, определённые в RDF/XML, содержат богатые модели знаний, но читать сырой XML — мучение. SimpleOntoDoc устраняет этот разрыв, генерируя аккуратный сайт документации — по аналогии с тем, что JavaDoc или Sphinx делают для кода, — но адаптированный специально для схем семантических веб-стандартов.

Проект был изначально создан для документирования **CIM (Common Information Model)** из стандартов энергосистем (IEC-61970) и работает с любой RDFS-совместимой онтологией.

---

## 🚀 Возможности

| Возможность | Описание |
|---|---|
| 📄 **Генерация HTML-сайта** | Создаёт полноценный статический HTML-сайт из одного или нескольких RDF/XML-файлов |
| 🔍 **Полнотекстовый поиск** | Клиентский JSON-индекс позволяет мгновенно найти любой класс или свойство |
| 📐 **UML-диаграммы** | SVG-диаграммы классов на основе PlantUML, встроенные на каждую страницу |
| 🏷️ **Поддержка стереотипов** | Автоматически классифицирует сущности: Class, Enum, DataType, Primitive, UnitSymbol и UnitMultiplier |
| 🔗 **Перекрёстные ссылки** | Каждая ссылка на домен/диапазон свойства ведёт на страницу соответствующей сущности |
| 📱 **Адаптивный дизайн** | Вёрстка на Bootstrap 5 работает на десктопе, планшете и мобильном устройстве |
| 🐳 **Docker-ориентированное развёртывание** | `publish.bash` собирает образ Docker с nginx, готовый к запуску |
| 🪟 **Поддержка Windows + WSL** | Автоматически использует WSL для команд Docker в Windows |

---

## 📸 Структура выходных файлов

Запуск SimpleOntoDoc на вашей онтологии создаёт статический сайт следующей структуры:

```
output/
├── index.html                         # Главная страница со статистикой
├── classes/_index.html                # Все классы
├── enums/_index.html                  # Все перечисления
├── datatypes/_index.html              # Все типы данных
├── primitives/_index.html             # Все примитивы
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

dotnet build RdfsBeautyDoc.csproj
```

### Публикация самодостаточного бинарника

```bash
dotnet publish RdfsBeautyDoc.csproj -c Release
```

Бинарник и все необходимые ресурсы будут помещены в `bin/Release/net9.0/publish/`.

---

## ⚙️ Конфигурация

SimpleOntoDoc настраивается полностью через **переменные окружения**.

| Переменная | Обязательна | Описание |
|---|---|---|
| `RDFSDOC_PATH_TO_RDFS` | ✅ | Путь к входному RDF/XML-файлу онтологии |
| `RDFSDOC_TITLE` | ✅ | Заголовок сайта документации |
| `RDFSDOC_DESCRIPTION` | ✅ | Краткое описание, отображаемое на главной странице |
| `RDFSDOC_COMMON_NAMESPACE` | ✅ | Префикс пространства имён в онтологии (например, `cim`) |
| `RDFSDOC_PLANTUML_URL` | ✅ | URL работающего сервера PlantUML (например, `http://localhost:55667`) |
| `RDFSDOC_OUTPUT_PATH` | ✅ | Директория, в которую будет записан сгенерированный сайт |
| `RDFSDOC_USE_NAMESPACE_FOR_PROPERTIES` | ❌ | Установите `true`, чтобы включить префикс пространства имён в названия свойств |

---

## 🏃 Использование

### Вариант 1 — Запуск через `publish.bash` (рекомендуется для продакшена)

`publish.bash` автоматизирует весь процесс: запускает контейнер PlantUML, генерирует сайт и упаковывает результат в образ Docker с nginx.

```bash
RDFSDOC_PATH_TO_RDFS=/path/to/ontology.xml \
RDFSDOC_TITLE="Моя RDFS-документация" \
RDFSDOC_DESCRIPTION="Автоматически сгенерированная документация для моей онтологии" \
RDFSDOC_COMMON_NAMESPACE=cim \
/usr/bin/bash publish.bash
```

Что произойдёт:
1. Будет скачан и запущен контейнер `plantuml/plantuml-server`
2. Запустится генератор документации
3. Будет собран образ Docker `nginx:alpine` со сгенерированным сайтом внутри
4. Образ будет отправлен в настроенный реестр

### Вариант 2 — Запуск бинарника напрямую

Сначала запустите сервер PlantUML (требуется Docker):

```bash
docker run -d --name plantuml -p 55667:8080 plantuml/plantuml-server
```

Затем запустите генератор:

```bash
RDFSDOC_PATH_TO_RDFS=/path/to/ontology.xml \
RDFSDOC_TITLE="Моя онтология" \
RDFSDOC_DESCRIPTION="Описание моей онтологии" \
RDFSDOC_COMMON_NAMESPACE=cim \
RDFSDOC_PLANTUML_URL=http://localhost:55667 \
RDFSDOC_OUTPUT_PATH=./output \
dotnet run --project RdfsBeautyDoc.csproj
```

Откройте `./output/index.html` в браузере для просмотра результата.

### Вариант 3 — Запуск через Docker

Готовый образ доступен по адресу:

```bash
docker run --rm \
  -v ./output:/out \
  --net=host \
  -e RDFSDOC_PATH_TO_RDFS=/out/ontology.xml \
  -e RDFSDOC_TITLE="Моя онтология" \
  -e RDFSDOC_DESCRIPTION="Описание моей онтологии" \
  -e RDFSDOC_COMMON_NAMESPACE=cim \
  -e RDFSDOC_PLANTUML_URL=http://localhost:55667 \
  -e RDFSDOC_OUTPUT_PATH=/out \
  gitea.simpleperson.ru/admin/rdfs-beauty-doc:latest
```

---

## 📖 Как это работает

```
RDF/XML файл(ы)
      │
      ▼
  XmlParse              ← Разбирает пространства имён, классы, свойства,
      │                    перечисления и стереотипы
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
├── Program.cs          # Точка входа и конфигурация
├── Model.cs            # Доменная модель: Class, Property, Description, Stereotype
├── XmlParse.cs         # Парсер RDF/XML
├── SiteGenerator.cs    # Движок генерации HTML (RazorLight)
├── PlantUML.cs         # Рендерер диаграмм PlantUML + управление Docker
├── ViewModel.cs        # View-модели для шаблонов Razor
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
└── RdfsBeautyDoc.csproj
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