using System;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class Vendorproduct
{
    public int Productid { get; set; }

    public int? Vendorid { get; set; }

    public string? Productname { get; set; }

    public int? Productweight { get; set; }

    public float? Productprice { get; set; }

    public virtual Vendor? Vendor { get; set; }
}
