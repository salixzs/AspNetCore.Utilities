# .Net 6 ASP.NET Minimal API

Minimal API (without Controllers) is a simplified approach starting with .Net 6 to create very basic minimal APIs.

Project here demonstrates how to use AspNetCore.Utilities features in this approach, but as it is simplistic - only few are utilized here with most minimal approach:

* Home (Start) page
* Error handling

There is also possibility to use configuration validation and Health Checking formatter in case you add those functionalities to minimal API - in this case look for implementation in full-scale (controller) implementation.

Page is Added as `MapGet("/")` endpoint handler

Error handling demands at least minimal implementation of base abstract class and to register it as middleware with `app`.