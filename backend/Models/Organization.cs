namespace backend.Models;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}
