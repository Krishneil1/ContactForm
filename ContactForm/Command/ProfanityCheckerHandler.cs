using ContactForm.Options;
using ContactForm.Query;
using MediatR;
using Microsoft.Extensions.Options;

namespace ContactForm.Command
{
    public class ProfanityCheckerHandler : IRequestHandler<CheckForProfanityQuery, bool>
    {
        public ProfanityWordsOptions ProfanityWordsOptions { get; }
        public ILogger Logger { get; }

        public ProfanityCheckerHandler(ILogger<SendContactFormHandler> logger, IOptionsMonitor<ProfanityWordsOptions> profanityWordOptionsMon)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ProfanityWordsOptions = profanityWordOptionsMon?.CurrentValue ?? throw new ArgumentNullException(nameof(profanityWordOptionsMon));
        }
        public Task<bool> Handle(CheckForProfanityQuery request, CancellationToken cancellationToken)
        {
            var words = ProfanityWordsOptions.Words;
            request.Message = request.Message!.ToLower();
            if(request.Message != null)
            {
                foreach(var word in words!)
                {
                    if(request.Message.Contains(word.ToLower()))
                    {
                        return Task.FromResult(true);
                    }
                }
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}