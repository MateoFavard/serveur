using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Server.Utils
{
	public class Email : MimeMessage, IDisposable
	{
		static private SmtpClient _smtpClient = new SmtpClient();
		static private BlockingCollection<Email> _messageQueue = new BlockingCollection<Email>();
		static private String _ADDRESS = Environment.GetEnvironmentVariable("BOT_EMAIL_ADDRESS") ?? "";
		static private String _PASSWORD = Environment.GetEnvironmentVariable("BOT_EMAIL_PASSWORD") ?? "";

		static public void SendMessages()
		{
			/*
			Email._smtpClient.Connect("iut-dijon.u-bourgogne.fr", 25, SecureSocketOptions.StartTls);
			Email._smtpClient.Authenticate(Email._ADDRESS, Email._PASSWORD);
			
			while (true)
			{
				Console.WriteLine("Waiting for emails to send ...");
				Email message = Email._messageQueue.Take();
				try {
					Console.WriteLine("Sending email ...");
					Email._smtpClient.Send(message);
					Console.WriteLine("Email sent successfully");
				}
				catch (Exception e)
				{
					Console.WriteLine("Couldn't send email: {0}", e);
				}
			}
			//*/
		}

		public Email(String recipient, String subject, String body) : base ()
		{
			this.From.Add(new MailboxAddress("Lord Of The Dungeons", Email._ADDRESS));
			this.To.Add(MailboxAddress.Parse(recipient));

			this.Subject = subject;

   			this.Body = new TextPart(TextFormat.Html) { Text = body };
		}

		public void Send()
		{
			Console.WriteLine("Email is ready to be sent");
			Email._messageQueue.Add(this);
		}
	}
}