using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class LicenseUsageItemModel
    {
        public int Index { get; set; }
        public string TickText { get; set; }
        public int TotalCount { get; set; }
        public int UsageCount { get; set; }
    }

    public class LicenseUsageItemCollection
    {
        public IEnumerable<LicenseUsageItemModel> Items { get; set; }

        public int Range { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

    }


}