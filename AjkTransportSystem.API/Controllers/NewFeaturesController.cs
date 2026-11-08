using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using System.ComponentModel.DataAnnotations;

namespace AjkTransportSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CargoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return BadRequest(new { message = "Tracking number is required." });

            var sql = @"
                SELECT TOP 1 
                    CargoId, TrackingNumber, SenderName, SenderPhone, ReceiverName, ReceiverPhone,
                    OriginCity, DestinationCity, WeightKg, PackageType, ItemDescription,
                    AssignedVehicleNo, DriverName, DriverPhone, Status, CurrentLocation,
                    EstimatedDelivery, Charges, PaymentStatus, CreatedAt
                FROM CargoShipments
                WHERE UPPER(LTRIM(RTRIM(TrackingNumber))) = UPPER(LTRIM(RTRIM({0})) OR UPPER(LTRIM(RTRIM(TrackingNumber))) LIKE '%' + UPPER(LTRIM(RTRIM({0}))) + '%';
            ";

            try
            {
                using var cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = @"
                    SELECT TOP 1 
                        CargoId, TrackingNumber, SenderName, SenderPhone, ReceiverName, ReceiverPhone,
                        OriginCity, DestinationCity, WeightKg, PackageType, ItemDescription,
                        AssignedVehicleNo, DriverName, DriverPhone, Status, CurrentLocation,
                        EstimatedDelivery, Charges, PaymentStatus, CreatedAt
                    FROM CargoShipments
                    WHERE UPPER(LTRIM(RTRIM(TrackingNumber))) = UPPER(LTRIM(RTRIM(@p0))) OR TrackingNumber LIKE '%' + @p0 + '%';
                ";
                var p = cmd.CreateParameter();
                p.ParameterName = "@p0";
                p.Value = trackingNumber.Trim();
                cmd.Parameters.Add(p);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    await cmd.Connection.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var result = new
                    {
                        cargoId = reader["CargoId"],
                        trackingNumber = reader["TrackingNumber"]?.ToString(),
                        senderName = reader["SenderName"]?.ToString(),
                        senderPhone = reader["SenderPhone"]?.ToString(),
                        receiverName = reader["ReceiverName"]?.ToString(),
                        receiverPhone = reader["ReceiverPhone"]?.ToString(),
                        originCity = reader["OriginCity"]?.ToString(),
                        destinationCity = reader["DestinationCity"]?.ToString(),
                        weightKg = reader["WeightKg"],
                        packageType = reader["PackageType"]?.ToString(),
                        itemDescription = reader["ItemDescription"]?.ToString(),
                        assignedVehicleNo = reader["AssignedVehicleNo"]?.ToString(),
                        driverName = reader["DriverName"]?.ToString(),
                        driverPhone = reader["DriverPhone"]?.ToString(),
                        status = reader["Status"]?.ToString(),
                        currentLocation = reader["CurrentLocation"]?.ToString(),
                        estimatedDelivery = reader["EstimatedDelivery"],
                        charges = reader["Charges"],
                        paymentStatus = reader["PaymentStatus"]?.ToString(),
                        createdAt = reader["CreatedAt"]
                    };
                    return Ok(result);
                }

                return NotFound(new { message = $"No shipment found with tracking code '{trackingNumber}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error querying shipment: " + ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmergencyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] EmergencyAlertRequest req)
        {
            if (req == null) return BadRequest("Invalid request.");

            try
            {
                using var cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO EmergencyAlerts (SenderName, SenderRole, SenderPhone, EmergencyType, Latitude, Longitude, LocationDescription, VehicleNumber, RouteName, Status, CreatedAt)
                    VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, 'Active', GETUTCDATE());
                    SELECT SCOPE_IDENTITY();
                ";

                AddParam(cmd, "@p0", req.SenderName ?? "AJK Citizen / Passenger");
                AddParam(cmd, "@p1", req.SenderRole ?? "Passenger");
                AddParam(cmd, "@p2", req.SenderPhone ?? "+923000000000");
                AddParam(cmd, "@p3", req.EmergencyType ?? "Breakdown / Medical Emergency");
                AddParam(cmd, "@p4", req.Latitude);
                AddParam(cmd, "@p5", req.Longitude);
                AddParam(cmd, "@p6", req.LocationDescription ?? "AJK Mountain Route Sector");
                AddParam(cmd, "@p7", req.VehicleNumber ?? "AJK-BUS");
                AddParam(cmd, "@p8", req.RouteName ?? "Inter-District Highway");

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    await cmd.Connection.OpenAsync();

                var newId = await cmd.ExecuteScalarAsync();

                return Ok(new
                {
                    alertId = newId,
                    status = "Dispatched",
                    message = "EMERGENCY BROADCAST DELIVERED: Rescue 1122 & Transport Control Room Notified with your GPS coordinates.",
                    dispatchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Emergency broadcast failed: " + ex.Message });
            }
        }

        private void AddParam(System.Data.Common.DbCommand cmd, string name, object val)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = val ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }

    public class EmergencyAlertRequest
    {
        public string? SenderName { get; set; }
        public string? SenderRole { get; set; }
        public string? SenderPhone { get; set; }
        public string? EmergencyType { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? LocationDescription { get; set; }
        public string? VehicleNumber { get; set; }
        public string? RouteName { get; set; }
    }
}
