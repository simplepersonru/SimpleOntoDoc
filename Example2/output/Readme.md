
# Описание

Minimal diverse HJSON example for Markdown generation

# Статистика

| Наименование | Количество |
|--------------|------------|
| Классы       | 3 |
| Перечисления | 1 |
| Примитивы    | 1 |
| Типы данных  | 1 |
| Составные типы | 1 |


# Полная диаграмма классов

```plantuml
@startuml
skinparam groupInheritance 6
set separator none
class "Thing" as demo.Thing  
class "Person" as demo.Person  
class "Device" as demo.Device  
demo.Person o--"0..*" demo.Device
demo.Thing <|-down- demo.Person
demo.Device ..> demo.Person
demo.Device o--"0..1" demo.Person
demo.Thing <|-down- demo.Device

@enduml
```

# Классы

## Легенда
🟦 - класс  
🟪 - перечисление  
🟧 - примитив  
🟨 - тип данных  
🟥 - составной тип  

| Идентификатор | Наименование | Описание |
|---------------|--------------|----------|
| 🟧 [String](./entities/String.md) | String primitive | Primitive text value |
| 🟨 [Date](./entities/Date.md) | Date datatype | Date value |
| 🟪 [StatusKind](./entities/StatusKind.md) | Device status enum | Device lifecycle states |
| 🟥 [GeoPoint](./entities/GeoPoint.md) | Geo point compound | Compound coordinates value |
| 🟦 [Thing](./entities/Thing.md) | Base thing class | Base domain entity |
| 🟦 [Person](./entities/Person.md) | Person class | Person who owns devices |
| 🟦 [Device](./entities/Device.md) | Device class | Trackable device |

Сделано с помощью [SimpleOntoDoc](https://github.com/simplepersonru/SimpleOntoDoc)      
