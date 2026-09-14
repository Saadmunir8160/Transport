namespace backend.DTOs;

public class SearchTripRequestDto
{
    public Guid OriginStopId { get; set; }
    public Guid DestinationStopId { get; set; }
    public DateTime Date { get; set; }
}
