using System;
using System.ComponentModel.DataAnnotations;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class UsageFilterModel
    {
        [Required(ErrorMessage = "View is required")]
        [Display(Name = "View")]
        public int ViewType { get; set; }

        [Required(ErrorMessage = "Range is required")]
        [Display(Name = "Range")]
        public int Range { get; set; }

        [Required(ErrorMessage = "From date is required")]
        [Display(Name = "From")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; }

        [Required(ErrorMessage = "To date is required")]
        [Display(Name = "To")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; }
    }
}