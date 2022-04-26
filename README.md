# QuickConstructor

QuickConstructor is a source generator that automatically creates a constructor from the fields and properties of a class.

## Features

- Decorate any class with the `[QuickConstructor]` attribute to automatically generate a constructor for that class.
- The constructor updates in real-time as the class is modified.
- Customize which fields and properties are initialized in the constructor.
- Generate null checks automatically based on nullable annotations.
- Works with nested classes and generic classes.
- Easily place attributes on the parameters of the generated constructor.
- Generate XML documentation automatically for the constructor.
- Lightning fast thanks to the .NET 6.0 incremental source generator system.

## Example

## Installation

The requirements to use the QuickConstructor package are the following:

- Visual Studio 17.0+
- .NET SDK 6.0.100+

Install the NuGet package:

```
dotnet add package QuickConstructor
```
