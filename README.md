# Sniccky

A web application built with .NET Core 3.1 that emulates a Snickers machine.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

## Prerequisites

* .NET Core 3.1

## Installation and Execution

1. Folllow [this guide](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio) to install and configure the .NET Core 3.1 Framework and Visual Studio.
2. Clone the repository.

```
projects> git clone https://github.com/r3core/Snickky.git
projects> cd Snickky
```

3. Execute the app with the dotnet CLI.

```
projects\Snickky> dotnet run --project .\Snickky\
```

## Routes

- http://{app-url}/Index - Customer Page
- http://{app-url}/Stocks - Operator Page

## References

- [Vending Machine Change Problem](https://putridparrot.com/blog/the-vending-machine-change-problem/)
- [Change-Making Problem](https://en.wikipedia.org/wiki/Change-making_problem)
- [ppaska/VendingMachine](https://github.com/ppaska/VendingMachine)
