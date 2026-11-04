using System.Net;
using System.Net.Mail;

namespace AjkTransportSystem.API.Services
{
    public class NotificationService
    {
        // Email Configuration - Apni Gmail credentials yahan dalo
        private static readonly string SmtpHost = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;
        private static readonly string SenderEmail = "your-email@gmail.com"; // CHANGE THIS
        private static readonly string SenderPassword = "your-app-password"; // CHANGE THIS (Use App Password, not regular password)
        
        // Twilio Configuration - Apne Twilio credentials yahan dalo
        private static readonly string TwilioAccountSid = "your-twilio-account-sid"; // CHANGE THIS
        private static readonly string TwilioAuthToken = "your-twilio-auth-token"; // CHANGE THIS
        private static readonly string TwilioWhatsAppNumber = "whatsapp:+14155238886"; // Twilio sandbox number

        public static string SendBookingConfirmation(string passengerName, string phone, string email, int bookingId, string routeName, DateTime travelDate, int seats, decimal totalFare)
        {
            var message = $@"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎫 AJK TRANSPORT - BOOKING CONFIRMED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Dear {passengerName},

Your bus ticket has been booked successfully!

📋 BOOKING DETAILS:
━━━━━━━━━━━━━━━━━━━━
🆔 Booking ID: #{bookingId}
🚌 Route: {routeName}
📅 Travel Date: {travelDate:dd MMM yyyy}
🪑 Seats Booked: {seats}
💰 Total Fare: Rs. {totalFare}
📱 Phone: {phone}
📧 Email: {email}
✅ Status: CONFIRMED

━━━━━━━━━━━━━━━━━━━━

⚠️ IMPORTANT INSTRUCTIONS:
• Please arrive 15 minutes before departure
• Carry a valid ID proof (CNIC)
• Show this booking confirmation at counter
• Keep your phone number active for updates

📞 For queries: 0300-1234567
🌐 Website: www.ajktransport.com

Thank you for choosing AJK Transport!
Safe Journey! 🛣️

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
";
            
            Console.WriteLine("\n" + message);
            
            // Send real email
            bool emailSent = SendEmail(email, $"Booking Confirmation #{bookingId}", message);
            
            // Send real WhatsApp
            bool whatsappSent = SendWhatsApp(phone, message);
            
            return message;
        }

        private static bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var smtpClient = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SenderEmail, "AJK Transport System"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(toEmail);
                    
                    smtpClient.Send(mailMessage);
                    Console.WriteLine($"\n[EMAIL] ✓ Email sent successfully to {toEmail}\n");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[EMAIL] ✗ Failed to send email: {ex.Message}");
                Console.WriteLine($"[EMAIL] Configure email settings in NotificationService.cs\n");
                return false;
            }
        }

        private static bool SendWhatsApp(string phone, string message)
        {
            try
            {
                // WhatsApp via Twilio (Install package: dotnet add package Twilio)
                // For now, showing simulation
                
                Console.WriteLine($"\n[WHATSAPP] ━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine($"[WHATSAPP] 📱 Sending WhatsApp to: {phone}");
                Console.WriteLine($"[WHATSAPP] ━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine($"[WHATSAPP] Message Preview:");
                Console.WriteLine(message);
                Console.WriteLine($"[WHATSAPP] ━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine($"[WHATSAPP] ⚠️  To enable real WhatsApp:");
                Console.WriteLine($"[WHATSAPP]    1. Run: dotnet add package Twilio");
                Console.WriteLine($"[WHATSAPP]    2. Get Twilio credentials from twilio.com");
                Console.WriteLine($"[WHATSAPP]    3. Update NotificationService.cs");
                Console.WriteLine($"[WHATSAPP] ━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[WHATSAPP] ✗ Error: {ex.Message}\n");
                return false;
            }
        }
    }
}
