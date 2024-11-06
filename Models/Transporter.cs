using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class Transporter
{
    public int Transporterid { get; set; }

    public int? Userid { get; set; }

    public string? Transportername { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual User? User { get; set; }
}
