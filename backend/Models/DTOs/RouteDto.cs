namespace backend.Models.DTOs;

public class RouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateRouteDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class StopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class CreateStopDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class RouteStopDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int StopOrder { get; set; }
    public StopDto? Stop { get; set; }
}

public class CreateRouteStopDto
{
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int StopOrder { get; set; }
}
