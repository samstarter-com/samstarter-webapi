using System.ComponentModel;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public enum BarChartRangeType
    {
        [Description("Day")] Day = 1,
        [Description("Week")] Week = 2,
        [Description("Month")] Month = 3,
        [Description("Quarter")] Quarter = 4,
        [Description("Year")] Year = 5
    }
}