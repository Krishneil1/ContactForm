using ContactForm.Models;
using ContactForm.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContactForm.Services
{
    public class ContactFormService : IContactFormService
    {
        public Task<IActionResult> ValidateContactForm(Form contactForm, int? storedAnswer, string storedCaptchaCreated, string storedCaptchaGuid)
        {
            if (storedCaptchaGuid != contactForm.Guid.ToString())
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Captcha token is incorrect"));
            }

            if (!storedAnswer.HasValue || string.IsNullOrEmpty(storedCaptchaCreated))
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Captcha answer is missing or expired"));
            }

            var answer = storedAnswer.Value;
            var captchaCreated = DateTime.Parse(storedCaptchaCreated);

            if (contactForm.Answer != answer)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Captcha answer is incorrect"));
            }

            if (DateTime.Now.Subtract(captchaCreated).TotalMinutes > 10)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Captcha has expired. Please refresh the page."));
            }

            if (DateTime.Now.Subtract(contactForm.DateTime).TotalSeconds < 10)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Form was submitted too fast"));
            }

            if (contactForm.Message!.Length < 10 || contactForm.Message!.Length > 140)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Message length is invalid"));
            }

            return Task.FromResult<IActionResult>(new OkResult());
        }
    }
}
