namespace Blockchain_Supply_Chain_Tracking_System.Models
{
    public class VerifyBlockSignatureRequest
    {
        public string blockchainId { get; set; }
        public int blockNumber { get; set; }
    }
}
