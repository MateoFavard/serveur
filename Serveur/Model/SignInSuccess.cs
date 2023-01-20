namespace Server.Model
{
	class SignInSuccess
	{
		public bool? Validated { get; set; } = null;

		public String? SessionToken { get; set; } = null;
	}
}
