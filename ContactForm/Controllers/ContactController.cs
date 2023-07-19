using ContactForm.Models;
using ContactForm.Options;
using ContactForm.Query;
using ContactForm.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ContactForm.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IMediator _mediator;
        private const string AnswerSessionKey = "CaptchaAnswer";
        private const string CaptchaCreatedSessionKey = "CaptchaCreated";
        public const string CaptchaGuidSessionKey = "CaptchaGuid";
        public IContactFormService ContactFormService;
        private IMemoryCache Cache;
        public CaptchaOptions CaptchaOptions { get; }
        public ContactController(IMediator mediator,IMemoryCache cache, IContactFormService contactFormService, IOptionsMonitor<CaptchaOptions> captchaOptionsMon)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            ContactFormService = contactFormService ?? throw new ArgumentNullException(nameof(contactFormService));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            CaptchaOptions = captchaOptionsMon.CurrentValue ?? throw new ArgumentNullException(nameof(captchaOptionsMon)); ;
        }
        ///  <response code="200">Returned if successful</response>
        ///  <response code="400">Returned if unsuccessful</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        [Route("GetCaptcha")]
        public async Task<IActionResult> GetCaptcha()
        {
            // Get the IP address of the user
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            
            // Create a cache key based on the IP address and current date
            var cacheKey = $"{ipAddress}_{DateTime.UtcNow.Date}";

            // Check if the cache already contains the cache key
            if (!Cache.TryGetValue(cacheKey, out int callCount))
            {
                // If the cache key doesn't exist, initialize it with a call count of 1 and set its expiration to the end of the day
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTimeOffset.UtcNow.Date.AddDays(1) - DateTimeOffset.UtcNow);

                Cache.Set(cacheKey, 1, cacheEntryOptions);
            }
            else
            {
                // If the cache key already exists, increment the call count
                callCount++;

                // If the call count exceeds the limit of 10, return an error response
                if (callCount > CaptchaOptions.LimitPerDay)
                {
                    return BadRequest("Exceeded the maximum number of captchas per day for this IP address.");
                }

                // Update the cache entry with the incremented call count
                Cache.Set(cacheKey, callCount);
            }

            // Generate and save the captcha
            var captcha = await _mediator.Send(new GetCaptchaQuery());

            HttpContext.Session.SetInt32(AnswerSessionKey, captcha.Answer);
            HttpContext.Session.SetString(CaptchaCreatedSessionKey, captcha.CreatedDate.ToString());
            HttpContext.Session.SetString(CaptchaGuidSessionKey, captcha.Guid.ToString());

            return Ok(captcha);
        }
        ///  <response code="200">Returned if successful</response>
        ///  <response code="400">Returned if unsuccessful</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("SendContactForm")]
        public async Task<IActionResult> SendContactForm(Form contactForm)
        {
            var storedAnswer = HttpContext.Session.GetInt32(AnswerSessionKey);
            var storedCaptchaCreated = HttpContext.Session.GetString(CaptchaCreatedSessionKey);
            var storedCaptchaGuid = HttpContext.Session.GetString(CaptchaGuidSessionKey);
            //validate the contact form
            if(storedAnswer == null || storedCaptchaCreated == null || storedCaptchaGuid == null)
            {
                return BadRequest("Captcha not found: Please refresh the page and try again.");
            }
            var validateContactForm = await ContactFormService.ValidateContactForm(contactForm, (int)storedAnswer!, storedCaptchaCreated!, storedCaptchaGuid!);
            if (validateContactForm is BadRequestObjectResult)
            {
                return validateContactForm;
            }
            //check if message contains any profanity
            bool containsProfanity = await _mediator.Send(new CheckForProfanityQuery { Message = contactForm.Message });
            if (containsProfanity)
            {
                return BadRequest("Message contains profanity");
            }
            await _mediator.Send(contactForm);
            //clear the session
            HttpContext.Session.Remove(AnswerSessionKey);
            HttpContext.Session.Remove(CaptchaCreatedSessionKey);
            HttpContext.Session.Remove(CaptchaGuidSessionKey);
            return Ok();
        }
    }
}
