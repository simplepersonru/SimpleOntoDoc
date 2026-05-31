
# Описание

**Устройство**  
Trackable device


# Сводка

| Ключ    | Значение |
|-----------------|------------|
| Тип             | 🟦 Class |
| namespace       | demo |
| Базовый класс | [Thing](Thing.md) |
| Свойств | 3 |
| Всех свойств | 5 |
| Дочерних классов | 0 |
| Ссылок       | 1 |



# Диаграмма

```plantuml
@startuml
skinparam groupInheritance 6
set separator none
annotation "Легенда" {
  #ссылка на enum
  ~ссылка на класс
  +простое свойство
}
class "Device \n **Устройство**" as demo.Device  
enum "StatusKind \n Device status enum" as demo.StatusKind  
class "Person \n Чувак" as demo.Person  
class "Thing \n Базовая штука" as demo.Thing  
demo.Device : #status : StatusKind
demo.Device : ~owner : Person
demo.Device : +location : GeoPoint
demo.Device ..> demo.Person
enum demo.StatusKind {
#Draft
#Active
}
demo.Device::status -- demo.StatusKind
demo.Device::owner o--"0..1" demo.Person
demo.Thing <|-down- demo.Device
demo.Thing : +id : String
demo.Thing : +createdAt : Date

@enduml
```


# Свойства

| Идентификатор  | Тип  | Ограничения | Display  | Описание  |
|----------------|------|------------ |-----------|-----------|
| <a name="status"/> [status](Device.md#status) | 🟪 [StatusKind](StatusKind.md) | _multiplicity_: 1<br/>  |  | Current status |
| <a name="owner"/> [owner](Device.md#owner) | 🟦 [Person](Person.md) | _multiplicity_: 0..1<br/>  |  | Device owner |
| <a name="location"/> [location](Device.md#location) | 🟥 [GeoPoint](GeoPoint.md) | _multiplicity_: 0..1<br/>  |  | Current location |



# Все свойства (включая унаследованные)

| Идентификатор | Тип   |  Ограничения  | Display   |  Описание |
| ---------------| -----| --------------|  ----------| ----------|
| [Thing.id](Thing.md#id) |  🟧 [String](String.md) | _multiplicity_: 1<br/> _pattern_: ^[A-Z0-9_-]{3,20}$<br/>  |  | External identifier |
| [Thing.createdAt](Thing.md#createdAt) |  🟨 [Date](Date.md) |  |  | Creation timestamp |
| [Device.status](Device.md#status) |  🟪 [StatusKind](StatusKind.md) | _multiplicity_: 1<br/>  |  | Current status |
| [Device.owner](Device.md#owner) |  🟦 [Person](Person.md) | _multiplicity_: 0..1<br/>  |  | Device owner |
| [Device.location](Device.md#location) |  🟥 [GeoPoint](GeoPoint.md) | _multiplicity_: 0..1<br/>  |  | Current location |


# Ссылки

| Свойство  | Display  | Описание |
| ----------| ----------|----------|
| [Person.devices](Person.md#devices) |  | Owned devices |

---
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
- пропуск места, чтобы ссылки попадали куда надо
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  
-  

Сделано с помощью [SimpleOntoDoc](https://github.com/simplepersonru/SimpleOntoDoc)  