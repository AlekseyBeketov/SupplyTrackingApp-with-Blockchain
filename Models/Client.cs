using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class Client
{
    public int Clientid { get; set; }

    public int? Userid { get; set; }

    public string? Clientname { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual User? User { get; set; }
}
