# Email & WhatsApp Setup Instructions

## ✉️ EMAIL SETUP (Gmail)

### Step 1: Gmail App Password banao
1. Google Account me jao: https://myaccount.google.com/
2. Security section me jao
3. "2-Step Verification" enable karo (agar nahi hai)
4. "App passwords" search karo
5. "Select app" → "Mail" choose karo
6. "Select device" → "Other" choose karo, "AJK Transport" likho
7. "Generate" button dabao
8. 16-digit password copy karo

### Step 2: NotificationService.cs me credentials dalo
```csharp
private static readonly string SenderEmail = "your-email@gmail.com"; // Apni Gmail ID
private static readonly string SenderPassword = "xxxx xxxx xxxx xxxx"; // 16-digit App Password
```

### Step 3: Test karo
- Booking karo aur check karo email aaya ya nahi

---

## 📱 WHATSAPP SETUP (Twilio)

### Step 1: Twilio Account banao
1. https://www.twilio.com/try-twilio pe jao
2. Sign up karo (free trial)
3. Phone number verify karo

### Step 2: WhatsApp Sandbox setup karo
1. Twilio Console me jao
2. "Messaging" → "Try it out" → "Send a WhatsApp message"
3. Sandbox number milega: +1 415 523 8886
4. Apne phone se WhatsApp karo us number ko
5. Join code bhejo (e.g., "join <your-code>")

### Step 3: Credentials copy karo
1. Twilio Console → Account Info
2. Account SID copy karo
3. Auth Token copy karo

### Step 4: Twilio package install karo
```bash
cd c:\Users\hp\OneDrive\Desktop\AjkTransportSystem.API\AjkTransportSystem.API
dotnet add package Twilio
```

### Step 5: NotificationService.cs me credentials dalo
```csharp
private static readonly string TwilioAccountSid = "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
private static readonly string TwilioAuthToken = "your_auth_token_here";
private static readonly string TwilioWhatsAppNumber = "whatsapp:+14155238886";
```

### Step 6: Code uncomment karo
NotificationService.cs me SendWhatsApp method me commented code ko uncomment karo

### Step 7: Test karo
- Booking karo aur check karo WhatsApp message aaya ya nahi

---

## 🚀 QUICK START (Email Only)

Agar sirf email chahiye (WhatsApp nahi):

1. Gmail App Password banao (upar dekho)
2. NotificationService.cs me email credentials dalo
3. Build karo: `dotnet build`
4. Run karo: `dotnet run`
5. Booking karo aur email check karo

---

## ⚠️ IMPORTANT NOTES

- **Gmail**: Regular password nahi chalega, App Password chahiye
- **Twilio**: Free trial me limited messages hai
- **WhatsApp**: Sandbox mode me sirf verified numbers ko message ja sakta hai
- **Production**: Twilio paid account aur WhatsApp Business API chahiye

---

## 🔧 TROUBLESHOOTING

**Email nahi ja raha:**
- App Password sahi hai?
- 2-Step Verification enabled hai?
- Internet connection theek hai?
- Gmail "Less secure app access" disabled hai? (App Password use karo)

**WhatsApp nahi ja raha:**
- Twilio package installed hai?
- Credentials sahi hain?
- Phone number WhatsApp sandbox me join kiya hai?
- Phone number format: +923001234567 (country code ke saath)

---

## 📞 SUPPORT

Agar koi problem ho to:
1. Console me error message dekho
2. Credentials double-check karo
3. Internet connection check karo
