using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Services;
using VerificationServiceProvider.Models;
using VerificationServiceProvider.Helpers;
using EmailServiceProvider;
using JwtTokenServiceProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMemoryCache();

builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection("EmailVerificationOptions"));

builder.Services.AddGrpcClient<JwtTokenServiceContract.JwtTokenServiceContractClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:JwtTokenServiceProvider"]!);
});
builder.Services.AddGrpcClient<EmailServicer.EmailServicerClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:EmailServiceProvider"]!);
});

builder.Services.AddTransient<IVerificationEmailFactory, VerificationEmailFactory>();
builder.Services.AddTransient<ICodeGenerator, VerificationCodeGenerator>();
builder.Services.AddTransient<IVerificationCacheHandler, VerificationCacheHandler>();
builder.Services.AddScoped<IJwtTokenServiceClient, JwtTokenServiceClientWrapper>();
builder.Services.AddScoped<IEmailServiceClient, EmailServiceClientWrapper>();

var app = builder.Build();

app.MapGrpcService<VerificationService>();
app.MapGet("/", () => 
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
