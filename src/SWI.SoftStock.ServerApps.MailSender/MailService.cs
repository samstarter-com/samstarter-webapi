using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;
using MimeKit.Text;
using SWI.SoftStock.Common.Dto2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.MailSender.Properties;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SWI.SoftStock.ServerApps.MailSender
{
    public class MailService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly ILogger logger;
        private readonly MailServiceOptions options;
        private readonly IVerificationService verificationService;
        private readonly CustomUserManager customUserManager;

        public MailService(ILogger<MailService> logger, MailServiceOptions options, IVerificationService verificationService
           , CustomUserManager customUserManager
            )
        {
            this.logger = logger as ILogger ?? NullLogger.Instance;
            this.options= options ?? throw new ArgumentNullException(nameof(options));
            this.verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            this.customUserManager= customUserManager ?? throw new ArgumentNullException(nameof(customUserManager));
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("MailService service running.");

            this.timer = new Timer(
                async (state) => { await this.SendEmail(); },
                null,
                TimeSpan.Zero,
                this.options.Interval);

            return Task.CompletedTask;
        }

        private async Task SendEmail()
        {
            var users = await this.verificationService.GetUsersForVerificationAsync();
            foreach (var user in users)
            {
                await SendAsync(user);
            }
        }

        private async Task SendAsync(User user)
        {
            this.logger.LogInformation($"Begin send for user Id: {user.Id}");
            await this.verificationService.SetUserVerificationStatusAsync(user, SendStatus.Sending);
            Response response;
            try
            {
                var status = this.verificationService.GetUserVerificationStatus(user);
                if (status != SendStatus.Sending)
                {
                    throw new Exception($"Wrong send status: {status} userid:{user.Id}");
                }
                var code = WebUtility.UrlEncode(await this.customUserManager.GenerateEmailConfirmationTokenAsync(user));
                var verifyUrl = $"{this.options.BaseAddress}/account/verify?userId={user.Id}&code={code}";
                response = this.SendNotification(user.Email, verifyUrl, user.Company.UniqueId);
            }
            catch (Exception e)
            {
                this.logger.LogError(0, e, e.Message);
                await this.verificationService.SetUserVerificationStatusAsync(user, SendStatus.Error, (byte)(user.SendCount + 1));
                throw;
            }

            await this.verificationService.SetUserVerificationStatusAsync(user,
                response?.Code == 0
                    ? SendStatus.Success
                    : user.SendCount > 2
                        ? SendStatus.MaxSendingCountError
                        : SendStatus.Error,
                (byte)(user.SendCount + 1));
        }

        private Response SendNotification(string email, string confirmationPath, Guid companyId)
        {
            if (companyId == null)
            {
                throw new ArgumentNullException(nameof(companyId));
            }


            var mailMessage = new MimeMessage();
            mailMessage.From.Add(MailboxAddress.Parse(this.options.SmtpClientOptions.UserName));
            mailMessage.To.Add(MailboxAddress.Parse(email));
            mailMessage.Subject = "SamStarter: Confirm your registration";
            mailMessage.Body = new TextPart(TextFormat.Html) { Text = Resources.UserVerificationMail_htm
                .Replace("<%ConfirmationPath%>", confirmationPath)
                .Replace("<%CompanyId%>", companyId.ToString())
            };

            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    smtpClient.Connect(this.options.SmtpClientOptions.Host, this.options.SmtpClientOptions.Port, true);
                    smtpClient.Authenticate(this.options.SmtpClientOptions.UserName, this.options.SmtpClientOptions.Password);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                catch (Exception e)
                {
                    this.logger.LogError(0, e, e.Message);
                    return new Response
                    {
                        Code = 6,
                        Message =
                            $"Exception:{e.Message}"
                    };
                }
            }
            return new Response
            {
                Code = 0
            };
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("MailService service is stopping.");

            this.timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.timer?.Dispose();
        }
    }
}