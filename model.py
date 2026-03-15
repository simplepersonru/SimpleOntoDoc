from __future__ import annotations
from dataclasses import dataclass, field
from typing import Optional, Dict, Any
from enum import Enum
import re

@dataclass
class RelInfo:
    id: str = str() # id атрибута в формате через точку, например "ProtectionEquipment.SensorWindings"
    description: str = str()
    multiplicity: str = str()

@dataclass
class Schema:
    """
    Онтологическая схема
    """
    classes: Dict[str, Class] = field(default_factory=dict) # описание классов онтологии

    def get_or_create(self, class_name: str) -> Class:
        if class_name in self.classes:
            return self.classes[class_name]
        new_class = Class(
            name=class_name
        )
        self.classes[class_name] = new_class
        return new_class
    
    def add_relation(self, lhs_prop_info: RelInfo, rhs_prop_info: RelInfo, addRight: bool = True, namespace: str = ""):
        left_domainStr, left_propName = lhs_prop_info.id.split('.', 1)
        left_class: Class = self.get_or_create(left_domainStr) 
        left_prop : Property = left_class.add_property(left_propName)

        right_domainStr, right_propName = rhs_prop_info.id.split('.', 1)
        right_class: Class = self.get_or_create(right_domainStr)
        if addRight:
            right_prop : Property = right_class.add_property(right_propName)

        left_prop.description = lhs_prop_info.description
        left_prop.range = right_class
        left_prop.multiplicity = lhs_prop_info.multiplicity
        if namespace != "":
            left_prop.namespace = namespace
        else:
            left_prop.namespace = left_class.namespace

        if addRight:
            right_prop.description = rhs_prop_info.description
            right_prop.range = left_class
            right_prop.multiplicity = rhs_prop_info.multiplicity
            if namespace != "":
                right_prop.namespace = namespace
            else:
                # не копипаста - берем из left_class по умолчанию, т.к. обычно связь пишется под левый класс (удобство) 
                right_prop.namespace = left_class.namespace 

            left_prop.inverse_role_name = right_prop.id
            right_prop.inverse_role_name = left_prop.id

@dataclass
class Base:
    """
    Базовый класс для всех классов онтологии
    """
    description: str = "" # Комментарий к объекту онтологии
    namespace: str = "cim" # Пространство имен объекта онтологии
    name: str = "" # Наименование объекта онтологии
    def toDict(self) -> Dict[str, Any]:
        """Преобразует Base в словарь"""
        result = {
            'description': self.description,
            'namespace': self.namespace,
            'name': self.name,
        }
        return result
    def validateDescription(self):
        self.description = self.description.replace("ï¿½", "-")
        self.description = self.description.replace("&#183;", "·")
        self.description = self.description.replace("&#176;", "°")
        self.description = self.description.replace("&#171;", "«")
        self.description = self.description.replace("&#187;", "»")
        self.description = self.description.replace("&#178;", "^2") # на самом деле это 2 написанная в верхней части строки, но простой текстовый редактор такого очевидно не покажет

        self.description = re.sub(r'<sup>(.*?)</sup>', r'^\1', self.description)
        self.description = re.sub(r'<sub>(.*?)</sub>', r'_\1', self.description)
        self.description = re.sub(r'<i>(.*?)</i>', r'\1', self.description)

@dataclass
class Attribute(Base):
    """
    База для Property и Enumerator
    """
    domain: Class = field(default=None) # Класс, которому принадлежит этот аттрибут (домен атрибута)
    initial_value: str = ""

    @property
    def id(self) -> str:
        """Аттрибут идентифицируется именем своего domain класса и затем собственным именем через точку"""
        return f"{self.domain.name}.{self.name}"
    
    def toDict(self) -> Dict[str, Any]:
        """Преобразует объект Attribute в словарь"""
        result = {}
        if self.initial_value != "":
            result['initial_value'] = self.initial_value

        return super().toDict() | result

@dataclass
class Property(Attribute):
    """
    Представляет свойство (атрибут или связь) класса в онтологической схеме.
    """

    multiplicity: str = "" # Ограничение кратности для свойства (например, "0..*", "1", и т.д.).
    inverse_role_name: str = "" # Имя обратного свойства, если связь двунаправленная.
    range: Class = field(default=None) # Тип свойства. Может быть примитивным типом, ссылкой на другой класс,... Но с точки зрения класса онтологии это всегда Class.
    optional: bool = True

    def toDict(self) -> Dict[str, Any]:
        """Преобразует объект Property в словарь"""
        result = {
            'range': self.range.name if self.range else None,
            'optional': self.optional,
        }

        if self.multiplicity != "":
            result['multiplicity'] = self.multiplicity
        if self.inverse_role_name != "":
            result['inverse_role_name'] = self.inverse_role_name

        return super().toDict() | result 

class Type(Enum):
    """
    Тип класса в онтологической схеме.
    """
    Class = "Class" # Обычный класс
    Enum = "Enum" # Класс с элементами перечислений
    Datatype = "Datatype" # Тип данных (содержит строго 3 свойства - value, unit, multiplier)
    Primitive = "Primitive" # Примитивный тип данных (Integer, Float, Date, ...)
    Compound = "Compound" # Составной тип данных

@dataclass
class Class(Base):
    """
    Представляет класс в онтологической схеме.
    """
    sub_class: Optional[Class] = None # Базовый класс (если есть)
    type: Type = Type.Class # Тип класса в онтологической схеме
    properties: Dict[str, Property] = field(default_factory=dict) # Словарь свойств класса
    enumerators: Dict[str, Enumerator] = field(default_factory=dict) # Словарь перечислений класса (для type == Type.Enum)

    @property
    def id(self) -> str:
        return self.name
    
    def add_property(self, name: str) -> Property:
        new_prop = Property(
            name=name,
            domain=self,
            namespace=self.namespace
        )
        self.properties[name] = new_prop
        return new_prop
        
    def add_enumerator(self, name: str, descr: str) -> Enumerator:
        new_enumerator = Enumerator(
            name=name,
            description=descr,
            domain=self,
            namespace=self.namespace
        )
        self.enumerators[name] = new_enumerator
        return new_enumerator

    def toDict(self) -> Dict[str, Any]:
        """Преобразует объект Class в словарь"""
        result = {
            'type': self.type.value if self.type else None,
            'sub_class': self.sub_class.name if self.sub_class else None,
        }
        if self.properties:
            result['properties'] = {}
            for prop_name, prop in self.properties.items():
                result['properties'][prop_name] = prop.toDict()
        if self.enumerators:
            result['enumerators'] = {}
            for enum_name, enum in self.enumerators.items():
                result['enumerators'][enum_name] = enum.toDict()
        return super().toDict() | result
    
@dataclass
class Enumerator(Attribute):
    """
    Представляет элемент перечисления в онтологической схеме.
    """

