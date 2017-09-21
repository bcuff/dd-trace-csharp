# DataDog Tracing for C#

## Packages

- [DataDog.Tracing](https://www.nuget.org/packages/DataDog.Tracing/) - Core library
- [DataDog.Tracing.Sql](https://www.nuget.org/packages/DataDog.Tracing.Sql/) - For tracing ADO .NET implementations. (e.g. SqlCommand, MySqlCommand, ...)

```bash
# Install from nuget.org
dotnet add package DataDog.Tracing
```


**Note** that only .NET Core is supported since APM is not currently supported on DataDog's Windows agent.


