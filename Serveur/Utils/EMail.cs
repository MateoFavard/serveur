using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Server.Utils
{
	public class Email : MimeMessage, IDisposable
	{
		static private SmtpClient smtpClient = new SmtpClient();
		static private BlockingCollection<Email> messageQueue = new BlockingCollection<Email>();

		static private String ADDRESS = Environment.GetEnvironmentVariable("BOT_EMAIL_ADDRESS") ?? "";
		static private String PASSWORD = Environment.GetEnvironmentVariable("BOT_EMAIL_PASSWORD") ?? "";

		private const int MAX_RETRIES = 5;
		static private TimeSpan STANDBY_DELAY = TimeSpan.FromMinutes(30);

		static public void SendMessages()
		{
			while (true)
			{
				Email? message = Email.messageQueue.Take();

				Console.WriteLine("[EMAIL] Waking up...");
				Email.smtpClient.Connect("iut-dijon.u-bourgogne.fr", 25, SecureSocketOptions.StartTls);
				Email.smtpClient.Authenticate(Email.ADDRESS, Email.PASSWORD);

				do
				{
					try
					{
						Console.WriteLine("[EMAIL] Sending message ...");
						Email.smtpClient.Send(message);
						Console.WriteLine("[EMAIL] Message sent successfully");
					}
					catch (Exception e)
					{
						Console.WriteLine("[EMAIL] Couldn't send message: {0}", e);
						if (++message.retries < MAX_RETRIES)
						{
							Console.WriteLine("[EMAIL] Retrying to send message (tries: {0})", message.retries);
							Email.messageQueue.Add(message);
						}
						else
                        {
							Console.WriteLine("[EMAIL] Maximum retry count exceeded");
						}
					}
				} while (Email.messageQueue.TryTake(out message, Email.STANDBY_DELAY));

				Email.smtpClient.Disconnect(true);
			}
		}

		private int retries;

		public Email(String recipient, String subject, String body) : base ()
		{
			this.From.Add(new MailboxAddress("Lord Of The Dungeons", Email.ADDRESS));
			this.To.Add(MailboxAddress.Parse(recipient));

			this.Subject = subject;

   			this.Body = new TextPart(TextFormat.Html) { Text = body };
		}

		public void Send()
		{
			Email.messageQueue.Add(this);
			Console.WriteLine("[EMAIL] Message ready to be sent");
		}
	}
}