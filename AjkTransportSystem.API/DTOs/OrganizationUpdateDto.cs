namespace AjkTransportSystem.API.DTOs
{
    public class OrganizationUpdateDto
    {
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
        public string? OfficeHours { get; set; }
        public string? DefaultLanguage { get; set; }
    }
}
