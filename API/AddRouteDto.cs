using System;

namespace API;

public class AddRouteDto
{
    public string AppName { get; set; }
    public string Host { get; set; }
    public string BackendAddress { get; set; } = "";
}
