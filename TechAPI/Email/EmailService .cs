using MimeKit;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;


namespace TechAPI.Email
{

    public interface IEmailService
    {
        Task SendAsync(List<(string Name, string Email)> student);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(List<(string Name, string Email)> students)
        {
            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]);

            foreach (var item in students)
            {
                var email = new MimeMessage();

                email.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
                email.To.Add(MailboxAddress.Parse(item.Email));

                email.Subject = "Today Attendance";

                email.Body = new TextPart("html")
                {
                    Text = $@"
                            <div style='font-family: Arial, sans-serif; background-color:#f4f6f8; padding:20px;'>
        
                                <div style='max-width:500px; margin:auto; background:white; padding:20px; border-radius:10px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
            
                                    <h3 style='color:green; text-align:center; margin-bottom:20px;'>
                                        Attendance Status
                                    </h3>

                                    <p style='font-size:14px; margin:8px 0;'>
                                        <b>Name:</b> {item.Name}
                                    </p>

                                    <p style='font-size:14px; margin:8px 0; color:red;'>
                                        <b>Status:</b> Absent
                                    </p>

                                    <p style='font-size:14px; margin:8px 0;'>
                                        <b>Date:</b> {DateTime.Now:dd-MM-yyyy}
                                    </p>

                                    <hr style='margin:20px 0;' />

                                    <p style='font-size:12px; color:gray; text-align:center;'>
                                        This is an automated message. Please do not reply.
                                    </p>

                                </div>
                            </div>"
                             };

                await smtp.SendAsync(email); 
            }

            await smtp.DisconnectAsync(true);
        }
    }
}
