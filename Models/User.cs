using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Publickey { get; set; } = null!;

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<Transporter> Transporters { get; set; } = new List<Transporter>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}
