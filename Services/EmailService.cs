using System.Net;
using System.Net.Mail;

namespace PlaygroundDashboard.Services;

public class EmailService(IConfiguration cfg)
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var host     = cfg["Email:SmtpHost"]    ?? throw new InvalidOperationException("Email:SmtpHost yapılandırılmamış.");
        var port     = int.Parse(cfg["Email:SmtpPort"] ?? "587");
        var username = cfg["Email:Username"]    ?? throw new InvalidOperationException("Email:Username yapılandırılmamış.");
        var password = cfg["Email:Password"]    ?? throw new InvalidOperationException("Email:Password yapılandırılmamış.");
        var from     = cfg["Email:FromAddress"] ?? username;
        var fromName = cfg["Email:FromName"]    ?? "Oyun Alanı";

#pragma warning disable SYSLIB0006 // SmtpClient eski API; küçük projeler için yeterli
        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl   = true
        };
#pragma warning restore SYSLIB0006

        using var msg = new MailMessage
        {
            From       = new MailAddress(from, fromName),
            Subject    = "Şifre Yenileme — Oyun Alanı",
            IsBodyHtml = true,
            Body       = $"""
                <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:24px">
                  <h2 style="color:#6c63ff;margin-bottom:16px">Şifre Yenileme</h2>
                  <p style="color:#333;margin-bottom:20px">
                    Şifrenizi yenilemek için aşağıdaki butona tıklayın.
                    Bu bağlantı <strong>1 saat</strong> geçerlidir.
                  </p>
                  <a href="{resetLink}"
                     style="display:inline-block;padding:12px 28px;
                            background:linear-gradient(135deg,#6c63ff,#a78bfa);
                            color:#fff;text-decoration:none;border-radius:8px;
                            font-weight:600;font-family:sans-serif">
                    Şifremi Yenile
                  </a>
                  <p style="color:#888;font-size:12px;margin-top:24px">
                    Şifre yenileme talebinde bulunmadıysanız bu e-postayı görmezden gelin.
                  </p>
                </div>
                """
        };
        msg.To.Add(toEmail);

        await client.SendMailAsync(msg);
    }
}
