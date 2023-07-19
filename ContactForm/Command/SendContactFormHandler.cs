using ContactForm.Models;
using ContactForm.Options;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace ContactForm.Command
{
    public class SendContactFormHandler : IRequestHandler<Form, bool>
    {
        public MailOptions MailOptions { get; }
        public ILogger Logger { get; }

        public SendContactFormHandler(ILogger<SendContactFormHandler> logger,  IOptionsMonitor<MailOptions> outlookOptionsMon)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MailOptions = outlookOptionsMon?.CurrentValue ?? throw new ArgumentNullException(nameof(outlookOptionsMon));
        }
        public async Task<bool> Handle(Models.Form request, CancellationToken cancellationToken)
        {
            request.Subject = $"Contact from {request.FirstName} {request.LastName}";

            try
            {
                using var email = new MailMessage
                {
                    //To and From addresses are required, even if they are the same (You can't send an email 'From' someone else email address)
                    To = { new MailAddress(MailOptions.ToEmail!) },//Add the email address you want to send to
                    From = new MailAddress(MailOptions.FromEmail!),//Add the email address you want to send from. Needs to be the same as the SMTP user
                    Subject = request.Subject,
                    IsBodyHtml = true
                };
                // Add the message body seperated out to make it easier to read
                email.Body = $"<p>From: {request.FirstName} {request.LastName}</p>" +
                             $"<p>Email: {request.Email}</p>" +
                             $"<p>Message: {request.Message}</p>";

                using var smtp = new SmtpClient(MailOptions.ServerName, MailOptions.Port)
                {
                    Credentials = new System.Net.NetworkCredential(MailOptions.FromEmail, MailOptions.AppPassword),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(email, cancellationToken);
                //Now send a copy of message to the sender
                email.To.Clear();
                email.To.Add(new MailAddress(request.Email!));
                //Add Subject "Your message has been received"
                email.Subject = "Your message has been received";
                // Define CSS styles for the box container
                string boxStyles = "border: 1px solid #ccc; padding: 10px; border-radius: 5px;";
                //Add the message body seperated out to make it easier to read
                email.Body = $@"
                    <div style='{boxStyles}'>
                        <p>Thank you for contacting us. We will get back to you as soon as possible.</p>
                        <h3>Copy of your Message</h3>
                        <br> <!-- Add line break here -->
                        <p>From: {request.FirstName} {request.LastName}</p>
                        <p>Email: {request.Email}</p>
                        <p>Message: {request.Message}</p>
                    </div>";
                await smtp.SendMailAsync(email, cancellationToken);


                Logger.LogInformation("***Contact Message*** Message sent successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send contact message");
                return false;
            }
            return true;
        }
    }
}
