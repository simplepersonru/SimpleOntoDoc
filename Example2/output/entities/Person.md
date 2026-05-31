
# Описание

Чувак  
Person who owns devices


# Сводка

| Ключ    | Значение |
|-----------------|------------|
| Тип             | 🟦 Class |
| namespace       | demo |
| Базовый класс | [Thing](Thing.md) |
| Свойств | 1 |
| Всех свойств | 3 |
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
class "Person \n Чувак" as demo.Person  
class "Device \n **Устройство**" as demo.Device  
class "Thing \n Базовая штука" as demo.Thing  
demo.Person : ~devices : Device
demo.Person::devices o--"0..*" demo.Device
demo.Thing <|-down- demo.Person
demo.Thing : +id : String
demo.Thing : +createdAt : Date

@enduml
```


# Свойства

| Идентификатор  | Тип  | Ограничения | Display  | Описание  |
|----------------|------|------------ |-----------|-----------|
| <a name="devices"/> [devices](Person.md#devices) | 🟦 [Device](Device.md) | _multiplicity_: 0..*<br/>  |  | Owned devices |



# Все свойства (включая унаследованные)

| Идентификатор | Тип   |  Ограничения  | Display   |  Описание |
| ---------------| -----| --------------|  ----------| ----------|
| [Thing.id](Thing.md#id) |  🟧 [String](String.md) | _multiplicity_: 1<br/> _pattern_: ^[A-Z0-9_-]{3,20}$<br/>  |  | External identifier |
| [Thing.createdAt](Thing.md#createdAt) |  🟨 [Date](Date.md) |  |  | Creation timestamp |
| [Person.devices](Person.md#devices) |  🟦 [Device](Device.md) | _multiplicity_: 0..*<br/>  |  | Owned devices |


# Ссылки

| Свойство  | Display  | Описание |
| ----------| ----------|----------|
| [Device.owner](Device.md#owner) |  | Device owner |

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