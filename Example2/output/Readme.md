
# Описание

Minimal diverse HJSON example for Markdown generation

# Статистика

| Наименование | Количество |
|--------------|------------|
| Классы       | 7 |
| Перечисления | 1 |
| Примитивы    | 1 |
| Типы данных  | 1 |
| Составные типы | 1 |


# Полная диаграмма классов

```plantuml
@startuml
skinparam groupInheritance 6
set separator none
class "Thing \n Базовая штука" as demo.Thing  
class "Person \n Чувак" as demo.Person  
class "Employee \n Работник" as demo.Employee  
class "Animal \n Животное" as demo.Animal  
class "Dog \n Собака" as demo.Dog  
class "Cat \n Кот" as demo.Cat  
class "Device \n **Устройство**" as demo.Device  
demo.Person o--"0..*" demo.Device
demo.Thing <|-down- demo.Person
demo.Person <|-down- demo.Employee
demo.Thing <|-down- demo.Animal
demo.Animal <|-down- demo.Dog
demo.Animal <|-down- demo.Cat
demo.Device .. demo.Person
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
| 🟧 [String](./entities/String.md) | Строка | Primitive text value |
| 🟨 [Date](./entities/Date.md) | Дата | Date value |
| 🟪 [StatusKind](./entities/StatusKind.md) | Device status enum | Device lifecycle states |
| 🟥 [GeoPoint](./entities/GeoPoint.md) | Geo point compound | Compound coordinates value |
| 🟦 [Thing](./entities/Thing.md) | Базовая штука | Base domain entity |
| 🟦 [Person](./entities/Person.md) | Чувак | Person who owns devices |
| 🟦 [Employee](./entities/Employee.md) | Работник | Работяга, который может владеть устройствами |
| 🟦 [Animal](./entities/Animal.md) | Животное | Животное! |
| 🟦 [Dog](./entities/Dog.md) | Собака | Собака! |
| 🟦 [Cat](./entities/Cat.md) | Кот | Кот! |
| 🟦 [Device](./entities/Device.md) | **Устройство** | Trackable device |

Сделано с помощью [SimpleOntoDoc](https://github.com/simplepersonru/SimpleOntoDoc)      
