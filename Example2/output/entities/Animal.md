
# Описание

Животное  
Животное!


# Сводка

| Ключ    | Значение |
|-----------------|------------|
| Тип             | 🟦 Class |
| namespace       | demo |
| Базовый класс | [Thing](Thing.md) |
| Свойств | 0 |
| Всех свойств | 2 |
| Дочерних классов | 2 |
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
class "Animal \n Животное" as demo.Animal  
class "Thing \n Базовая штука" as demo.Thing  
demo.Thing <|-down- demo.Animal
demo.Thing : +id : String
demo.Thing : +createdAt : Date

@enduml
```



# Дочерние классы

| Идентификатор  | Display  | Описание  |
| ---------------| ----------| ----------|
| [Dog](Dog.md) | Собака | Собака! |
| [Cat](Cat.md) | Кот | Кот! |


# Все свойства (включая унаследованные)

| Идентификатор | Тип   |  Ограничения  | Display   |  Описание |
| ---------------| -----| --------------|  ----------| ----------|
| [Thing.id](Thing.md#id) |  🟧 [String](String.md) | _multiplicity_: 1<br/> _pattern_: ^[A-Z0-9_-]{3,20}$<br/>  |  | External identifier |
| [Thing.createdAt](Thing.md#createdAt) |  🟨 [Date](Date.md) |  |  | Creation timestamp |


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