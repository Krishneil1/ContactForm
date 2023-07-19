using MediatR;

namespace ContactForm.Models
{
    public class Form : IRequest<bool>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public int Answer { get; set; }
        public DateTime DateTime { get; set; }
        public Guid Guid { get; set; }
    }
}
