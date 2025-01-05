namespace Blockchain_Supply_Chain_Tracking_System.Models
{
	public class TrackingModel
	{
		public List<User> Users { get; set; }
		public List<Vendor> Vendors { get; set; }
		public List<Vendorproduct> VendorProducts { get; set; }
		public List<Blockchainblock> Blockchainblocks { get; set; }
		public List<Blockchainid> Blockchainids { get; set; }
		public List<UserGroup> UserGroups { get; set; }
		public List<Transporter> Transporters { get; set; }
		public List<Client> Clients { get; set; }
	}
}
