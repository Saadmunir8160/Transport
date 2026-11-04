namespace AjkTransportSystem.API.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; } // Optional now, but can be kept
        public string Password { get; set; }
        public string Role { get; set; } // Passenger/Admin/User
        
        public string Phone { get; set; }
        public string LanguagePreference { get; set; }
        public string OTP { get; set; }
    }
    
    public class SendOtpDto
    {
        public string Phone { get; set; }
    }
}
