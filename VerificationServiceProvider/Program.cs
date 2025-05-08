using VerificationServiceProvider.Components;
using VerificationServiceProvider.Dtos;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Services;
using JwtTokenServiceClient = JwtTokenServiceProvider.JwtTokenServiceContract.JwtTokenServiceContractClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMemoryCache();

builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection("EmailVerificationOptions"));

builder.Services.AddGrpcClient<JwtTokenServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:JwtTokenServiceProvider"]!);
});

builder.Services.AddSingleton<IVerificationEmailFactory, VerificationEmailFactory>();
builder.Services.AddSingleton<ICodeGenerator, VerificationCodeGenerator>();
builder.Services.AddTransient<IVerificationCacheHandler, VerificationCacheHandler>();

var app = builder.Build();

app.MapGrpcService<VerificationService>();
app.MapGet("/", () => 
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
