namespace Blockchain_Supply_Chain_Tracking_System.Models
{
	public class CreateBlockchainRequest
	{
		public string BatchId { get; set; }
		public string UserGroupId { get; set; }
		public int UserId { get; set; }
		public string DetailsOrder { get; set; }
		public string Signature { get; set; }
		public string PublicKey { get; set; }
	}
}
