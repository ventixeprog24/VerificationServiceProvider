using EmailServiceProvider;
using Microsoft.Extensions.Options;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models;

namespace VerificationServiceProvider.Factories
{
    public class VerificationEmailFactory(IOptions<EmailVerificationOptions> options) : IVerificationEmailFactory
    {
        private readonly EmailVerificationOptions _settings = options.Value;

        public EmailRequest CreateVerificationEmail(VerificationEmailContentModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new EmailRequest
            {
                Recipients = { model.Email },
                SenderAddress = _settings.SenderAddress,
                Subject = $"Verification code: {model.Code}",
                PlainText = CreatePlainText(model),
                Html = CreateHtml(model)
            };
        }

        private string CreatePlainText(VerificationEmailContentModel model)
        {
            return $@"
                        Verify Your Email Address

                        Hello,

                        To complete your verification, please enter the following code:

                        {model.Code}

                        Alternatively, you can open the verification page using the following link:
                        {_settings.FrontendUrl}{_settings.VerificationPath}?email={model.Email}&token={model.Token}

                        If you did not initiate this request, you can safely disregard this email.
                        We take your privacy seriously. No further action is required if you did not initiate this request.

                        © {_settings.DomainUrl}. All rights reserved.
                        ";
        }

        private string CreateHtml(VerificationEmailContentModel model)
        {
            return $@"
                        <!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                            <title>Your verification code</title>
                        </head>
                        <body style='margin:0; padding:32px; font-family: Inter, sans-serif; background-color: #F7F7F7; color:#1E1E20;'>
                            <div style='max-width: 660px; margin: 32px auto; background: #FFFFFF; border-radius: 16px; padding: 32px;'>

                                <h1 style='font-size: 32px; font-weight: 600; color: #37437D; margin-bottom: 16px; text-align: center;'>
                                    Verify Your Email Address
                                </h1>

                                <p style='font-size: 16px; color: #1E1E20; margin-bottom: 16px;'>Hello,</p>

                                <p style='font-size: 16px; color: #1E1E20; margin-bottom: 24px;'>
                                    To complete your verification, please enter the code below or click the button to open a new page
                                </p>

                                <div style='display: flex; justify-content: center; align-items: center; padding: 16px; background-color: #FCD3FE; color: #1C2346; font-size: 32px; font-weight: 600; border-radius: 12px; margin-bottom: 24px; letter-spacing: 8px;'>
                                    {model.Code}
                                </div>

                                <div style='text-align: center; margin-bottom: 32px;'>
                                    <a href='{_settings.FrontendUrl}{_settings.VerificationPath}?email={model.Email}&token={model.Token}' style='background-color: #F26CF9; color: #FFFFFF; padding: 12px 24px; border-radius: 20px; text-decoration: none; display: inline-block;'>
                                        Open Verification Page
                                    </a>
                                </div>

                                <p style='font-size: 12px; color: #777779; text-align: center; margin-top: 24px;'>
                                    If you did not initiate this request, you can safely disregard this email.
                                    <br><br>
                                    We take your privacy seriously. No further action is required if you did not initiate this request.
                                    For more information about hoe we process personal data, please see our
                                    <a href='#' style='color: #F26CF9; text-decoration: none;'>Privacy Policy</a>
                                </p>

                                <div style='font-size: 12px; color: #777779; text-align: center; margin-top: 24px;'>
                                    © {_settings.DomainUrl}. All rights reserved.
                                </div>
                            </div>
                        </body>
                        </html> 
                        ";
        }
    }
}
