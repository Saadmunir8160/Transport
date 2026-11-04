namespace AjkTransportSystem.API.DTOs
{
    public class UploadDocumentDto
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string ReminderDays { get; set; } = "30,15,7";
        public string FileUrl { get; set; } = string.Empty;
    }
}
