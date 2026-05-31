
# Описание

Собака  
Собака!


# Сводка

| Ключ    | Значение |
|-----------------|------------|
| Тип             | 🟦 Class |
| namespace       | demo |
| Базовый класс | [Animal](Animal.md) |
| Свойств | 0 |
| Всех свойств | 2 |
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
class "Dog \n Собака" as demo.Dog  
class "Animal \n Животное" as demo.Animal  
class "Thing \n Базовая штука" as demo.Thing  
demo.Animal <|-down- demo.Dog
demo.Thing <|-down- demo.Animal
demo.Thing : +id : String
demo.Thing : +createdAt : Date

@enduml
```




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