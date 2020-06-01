# JonasPluginBase
Microsoft Dynamics CRM / 365 / CDS / Power Apps plugin base class

Easily consumed by adding this repository as a submodule to Your project with command:

`git submodule add https://github.com/rappen/JonasPluginBase`

Include the shared project in the `JonasPluginBase` folder, and add a reference to this shared project from Your plugin project.

## Contents

This library contains base classes `JonasPluginBase` and `JonasCodeActivityBase`.

My version of "LocalContext" is called `JonasPluginBag` and includes proxies for `IOrganizationService`, `ITracingService` etc, providing timings of calls, indentation helpers etc.

The project also contains some handy extension methods for `Microsoft.Xrm.Sdk.Entity` and `Microsoft.Xrm.Sdk.Query.QueryExpression`.
