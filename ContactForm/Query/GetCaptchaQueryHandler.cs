using ContactForm.Query;
using ContactForm.Models;
using MediatR;

namespace ContactForm.Query
{
    public class GetCaptchaQueryHandler : IRequestHandler<GetCaptchaQuery, Captcha>
    {
        public Task<Captcha> Handle(GetCaptchaQuery request, CancellationToken cancellationToken)
        {
            var captcha = new Captcha();
            captcha.OperandOne = new Random().Next(1, 15);
            captcha.OperandTwo = new Random().Next(1, 15);
            captcha.Answer = captcha.OperandOne + captcha.OperandTwo;
            captcha.CreatedDate = DateTime.Now;
            captcha.Guid = Guid.NewGuid();
            return Task.FromResult(captcha);
        }
    }
}
