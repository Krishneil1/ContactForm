using ContactForm.Middleware;
using ContactForm.Models;
using ContactForm.Options;
using ContactForm.Query;
using ContactForm.Services.Interfaces;
using ContactForm.Services;
using MediatR;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddTransient<IRequestHandler<GetCaptchaQuery, Captcha>, GetCaptchaQueryHandler>();
builder.Services.AddOptions<MailOptions>().Bind(builder.Configuration.GetSection("Mail"));
builder.Services.AddOptions<ApiKeyOptions>().Bind(builder.Configuration.GetSection("ApiKey"));
builder.Services.AddOptions<ClientOptions>().Bind(builder.Configuration.GetSection("Client"));
builder.Services.AddOptions<ProfanityWordsOptions>().Bind(builder.Configuration.GetSection("ProfanityWords"));
builder.Services.AddOptions<CaptchaOptions>().Bind(builder.Configuration.GetSection("Captcha"));
builder.Services.AddTransient<IContactFormService, ContactFormService>();

builder.Services.AddDistributedMemoryCache();

//Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2);

});
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Contact Form API", Version = "v1" });
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "ApiKey must appear in header",
            Type = SecuritySchemeType.ApiKey,
            Name = "XApiKey",
            In = ParameterLocation.Header,
            Scheme = "ApiKeyScheme"
        });
        var key = new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "ApiKey"
            },
            In = ParameterLocation.Header
        };
        var requirement = new OpenApiSecurityRequirement
                    {
                             { key, new List<string>() }
                    };
        c.AddSecurityRequirement(requirement);
    }
);

builder.Services.AddCors(
                opt =>
                {
                    opt.AddDefaultPolicy(
                        builder =>
                        {
                            builder
                                .WithOrigins(
                                "*"
                                )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                });
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseSession();
app.MapControllers();

app.Run();
