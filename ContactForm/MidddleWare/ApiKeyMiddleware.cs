using ContactForm.Options;
using Microsoft.Extensions.Options;

namespace ContactForm.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeyOptions _apiKeyOptions;
        private const string APIKEY = "XApiKey";
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IOptionsMonitor<ApiKeyOptions> optionsMonitor, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _apiKeyOptions = optionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        //You will need the API Key to access the API and Swagger UI (Swagger UI is a tool that visually presents the API) 
        //The API Key is stored in appsettings.json
        //The API Key is stored in appsettings.Development.json
        private async Task HandleUnauthorizedRequest(HttpContext context, string keyType, string errorMessage)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync(errorMessage);
            _logger.LogInformation($"Api Key was not provided or {keyType} Key Used");
        }

        public async Task Invoke(HttpContext context)
        {
            string url = context.Request.Path;

            if (!context.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
            {
                await HandleUnauthorizedRequest(context, "Key", "Api Key was not provided or Unauthorized client");
                return;
            }

            if (string.Equals(extractedApiKey, _apiKeyOptions.WebKey))
            {
                await _next(context);
            }
            else if (string.Equals(extractedApiKey, _apiKeyOptions.SwaggerKey))
            {
                await _next(context);
            }
            else
            {
                await HandleUnauthorizedRequest(context, "Key", "Unauthorized client");
                return;
            }
            return;
        }
    }
}
