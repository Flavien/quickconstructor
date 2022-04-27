# QuickConstructor
[![QuickConstructor](https://img.shields.io/nuget/v/QuickConstructor.svg?style=flat-square&color=blue&logo=nuget)](https://www.nuget.org/packages/QuickConstructor/)

QuickConstructor is a source generator that automatically creates a constructor from the fields and properties of a class.

## Features

- Decorate any class with the `[QuickConstructor]` attribute to automatically generate a constructor for that class.
- The constructor updates in real-time as the class is modified.
- Customize which fields and properties are initialized in the constructor.
- Generate null checks automatically based on nullable annotations.
- Works with nested classes and generic classes.
- Easily place attributes on the parameters of the generated constructor.
- No traces left after compilation, no runtime reference necessary.
- Generate XML documentation automatically for the constructor.
- Lightning fast thanks to the .NET 6.0 incremental source generator system.

## Example

Code without QuickConstructor:

```csharp
public class Car
{
    private readonly string _registration;
    private readonly string _model;
    private readonly string _make;
    private readonly string _color;
    private readonly int _year;

    public Car(string registration, string model, string make, string color, int year)
    {
        _registration = registration;
        _model = model;
        _make = make;
        _color = color;
        _year = year;
    }
}
```

With QuickConstructor, this becomes:

```csharp
[QuickConstructor]
public class Car
{
    private readonly string _registration;
    private readonly string _model;
    private readonly string _make;
    private readonly string _color;
    private readonly int _year;
}
```

The constructor is automatically generated.

## Installation

The requirements to use the QuickConstructor package are the following:

- Visual Studio 17.0+
- .NET SDK 6.0.100+

Install the NuGet package:

```
dotnet add package QuickConstructor
```

## Usage

QuickConstructor is very easy to use. By simply decorating a class with the `[QuickConstructor]` attribute, the source generator will automatically create a constructor based on fields and properties declared in the class. The constructor will automatically update to reflect any change made to the class.

QuickConstructor offers options to customize various aspects of the constructors being generated.

### Fields selection

Quick constructors will always initialize read-only fields as the constructor would otherwise cause a compilation error. However mutable fields can either be included or excluded from the constructor. This is controlled via the `Fields` property of the `[QuickConstructor]` attribute. The possible values are:

| Value                          | Description |
| ------------------------------ | ----------- |
| `IncludeFields.ReadOnlyFields` | **(default)** Only read-only fields are included in the constructor. |
| `IncludeFields.AllFields` | All fields are included in the constructor. |

Fields with an initializer are never included as part of the constructor.

### Properties selection

It is possible to control which property is initialized in the constructor via the `Properties` property of the `[QuickConstructor]` attribute. The possible values are:

| Value                    | Description |
| ------------------------ | ----------- |
| `IncludeProperties.None` | No property is initialized in the constructor. |
| `IncludeProperties.ReadOnlyProperties` | **(default)** Only read-only auto-implemented properties are included in the constructor. |
| `IncludeProperties.AllProperties` | All settable properties are included in the constructor. |

Properties with an initializer are never included as part of the constructor.

### Null checks

QuickConstructor has the ability to generate null checks for reference parameters. This is controlled via the `NullCheck` property of the `[QuickConstructor]` attribute. The possible values are:

| Value               | Description |
| ------------------- | ----------- |
| `NullChecks.Always` | Null checks are generated for any included field or property whose type is a reference type. |
| `NullChecks.Never` | Null checks are not generated for this constructor. |
| `NullChecks.NonNullableReferencesOnly` | **(default)** When null-state analysis is enabled (from C# 8.0), a null check will be generated if a type is marked as non-nullable. Otherwise no null check is generated for this parameter. |

### Constructor accessibility

It is possible to customize the accessibility level of the auto-generated constructor. This is controlled via the `ConstructorAccessibility` property of the `[QuickConstructor]` attribute.

## License

Copyright 2022 Flavien Charlon

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and limitations under the License.
