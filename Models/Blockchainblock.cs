using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class Blockchainblock
{
    public int Blockid { get; set; }

    public string? Blockchainid { get; set; }

    public int? Vendorid { get; set; }

    public int? Clientid { get; set; }

    public int? Transporterid { get; set; }

    public int? Userid { get; set; }

    public string? Batchid { get; set; }

    public string? Eventtype { get; set; }

    public string? Location { get; set; }

    public string? Hash { get; set; }

    public string? Previoushash { get; set; }

    public string? Carnumber { get; set; }

    public string? Eventdetails { get; set; }

    public DateTime? Timestampblock { get; set; }

    public int? Nonce { get; set; }

    public string? Publickey { get; set; }
}
