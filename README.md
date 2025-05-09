# VerificationServiceProvider
A gRPC-based microservice for handling **email verification** during user sign-up. It generates one-time codes, constructs email message, delegates token generation and validation to an external `JwtTokenService` and email delivery to an external `EmailService`.

## Dependencies
- `JwtTokenService`: External microservice used to generate and validate JWT tokens.
- `EmailService`: External microservice used to deliver verification emails.

## How to Use as Client
Make sure to include an identical copy of the `protos/verification.proto` file and configure it in the `.csproj` file.

### Required Packages
- Grpc.Net.Client
- Grpc.Net.ClientFactory
- Grpc.Tools
- Google.Protobuf

### Example Usage 
```csharp
//Generate gRPC channel and create client 
var channel = GrpcChannel.ForAddress("https://localhost:5000");
var client = new VerificationContract.VerificationContractClient(channel);

// Send verification code 
var sendReply = await client.SendVerificationCodeAsync(new SendVerificationCodeRequest 
{ 
    Email = "test@example.com" 
});

// Validate verification code
var validateCodeReply = await client.ValidateVerificationCode(new ValidateVerificationCodeRequest 
{ 
    Email = "test@example.com", 
    Code = "123456"
});

// Validate verification token
var validateTokenReply = await client.ValidateVerificationToken(new ValidateVerificationTokenRequest 
{ 
    Token = "some-verification-token"
});

//Example success response
{
    "succeeded": true,
    "message":"Verification email sent",
}

//Example failed response
{
    "succeeded": true,
    "error":"Failed to send verification email",
}
```