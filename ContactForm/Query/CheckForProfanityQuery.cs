using ContactForm.Models;
using MediatR;

namespace ContactForm.Query
{
    public class CheckForProfanityQuery : IRequest<bool>
    {
        public string ? Message { get; set; }
    }
}
