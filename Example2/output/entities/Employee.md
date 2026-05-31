
# Описание

Работник  
Работяга, который может владеть устройствами


# Сводка

| Ключ    | Значение |
|-----------------|------------|
| Тип             | 🟦 Class |
| namespace       | demo |
| Базовый класс | [Person](Person.md) |
| Свойств | 1 |
| Всех свойств | 4 |
| Дочерних классов | 0 |
| Ссылок       | 0 |



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
class "Employee \n Работник" as demo.Employee  
class "Person \n Чувак" as demo.Person  
class "Thing \n Базовая штука" as demo.Thing  
demo.Employee : +company : String
demo.Person <|-down- demo.Employee
demo.Person : ~devices : Device
demo.Thing <|-down- demo.Person
demo.Thing : +id : String
demo.Thing : +createdAt : Date

@enduml
```


# Свойства

| Идентификатор  | Тип  | Ограничения | Display  | Описание  |
|----------------|------|------------ |-----------|-----------|
| <a name="company"/> [company](Employee.md#company) | 🟧 [String](String.md) |  |  | Компания, в которой работает сотрудник |



# Все свойства (включая унаследованные)

| Идентификатор | Тип   |  Ограничения  | Display   |  Описание |
| ---------------| -----| --------------|  ----------| ----------|
| [Thing.id](Thing.md#id) |  🟧 [String](String.md) | _multiplicity_: 1<br/> _pattern_: ^[A-Z0-9_-]{3,20}$<br/>  |  | External identifier |
| [Thing.createdAt](Thing.md#createdAt) |  🟨 [Date](Date.md) |  |  | Creation timestamp |
| [Person.devices](Person.md#devices) |  🟦 [Device](Device.md) | _multiplicity_: 0..*<br/>  |  | Owned devices |
| [Employee.company](Employee.md#company) |  🟧 [String](String.md) |  |  | Компания, в которой работает сотрудник |


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