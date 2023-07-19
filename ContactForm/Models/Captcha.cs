namespace ContactForm.Models
{
    public class Captcha
    {
        public int OperandOne { get; set; }
        public int OperandTwo { get; set; }
        public int Answer { get; set; }
        public DateTime CreatedDate { get; set; }
        // This is the GUID that will be used to validate the captcha
        public Guid Guid { get; set; }
    }
}
