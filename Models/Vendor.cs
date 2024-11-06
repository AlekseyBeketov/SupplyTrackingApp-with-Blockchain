using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class Vendor
{
    public int Vendorid { get; set; }

    public int? Userid { get; set; }

    public string? Vendorname { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Vendorproduct> Vendorproducts { get; set; } = new List<Vendorproduct>();
}
