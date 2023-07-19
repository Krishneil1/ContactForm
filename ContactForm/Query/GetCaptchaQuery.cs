using ContactForm.Models;
using MediatR;

namespace ContactForm.Query
{
    public class GetCaptchaQuery : IRequest<Captcha>
    {
    }
}
