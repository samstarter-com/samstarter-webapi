using System.ComponentModel;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public enum UsageViewType
    {
        [Description("Chart")]
        Chart = 1,
        [Description("Table")]
        Table = 2
    }
}