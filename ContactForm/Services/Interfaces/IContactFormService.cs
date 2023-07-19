using ContactForm.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactForm.Services.Interfaces
{
    public interface IContactFormService
    {
        Task<IActionResult> ValidateContactForm(Form contactForm, int? storedAnswer, string storedCaptchaCreated, string storedCaptchaGuid);
    }
}
