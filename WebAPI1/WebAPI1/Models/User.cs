namespace WebAPI1.Models
{
	public class User
	{
		public string Id { get; set; }= string.Empty;
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public bool MarketingConsent { get; set; }
	}
}
