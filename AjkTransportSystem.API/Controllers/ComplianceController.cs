using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplianceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComplianceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var now = DateTime.UtcNow;
            var in30Days = now.AddDays(30);

            var allDocs = await _context.Documents.ToListAsync();

            var expired = allDocs.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now).ToList();
            var expiring30 = allDocs.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= in30Days).ToList();
            
            // For missing docs, we can do a mock approach
            var missingCount = 5; // Simplified for MVP

            return Ok(new
            {
                ExpiredAndBlocking = expired,
                ExpiringIn30Days = expiring30,
                MissingDocumentsCount = missingCount
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadOrReplaceDocument([FromBody] UploadDocumentDto dto)
        {
            var existing = await _context.Documents.FirstOrDefaultAsync(d => d.EntityType == dto.EntityType && d.EntityId == dto.EntityId && d.DocumentType == dto.DocumentType);

            if (existing != null)
            {
                existing.FileUrl = dto.FileUrl;
                existing.ExpiryDate = dto.ExpiryDate;
                existing.ReminderDays = dto.ReminderDays;
            }
            else
            {
                var doc = new Document
                {
                    EntityType = dto.EntityType,
                    EntityId = dto.EntityId,
                    DocumentType = dto.DocumentType,
                    FileUrl = dto.FileUrl,
                    ExpiryDate = dto.ExpiryDate,
                    ReminderDays = dto.ReminderDays
                };
                _context.Documents.Add(doc);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Document uploaded successfully." });
        }

        [HttpGet("export")]
        public IActionResult ExportComplianceReport()
        {
            return Ok(new { url = "https://example.com/reports/compliance_report.xlsx" });
        }
    }
}
