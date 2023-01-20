namespace Server.Model
{
	public class SignUpRequest
	{
		public String Username { get; set; } = "";
		public String Email { get; set; } = "";
		public String CaptchaToken { get; set; } = "";
	}
}