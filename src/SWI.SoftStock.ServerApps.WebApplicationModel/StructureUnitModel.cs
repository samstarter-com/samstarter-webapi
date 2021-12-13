namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using SWI.SoftStock.ServerApps.DataModel2;

    public class StructureUnitModel
    {      
        public Guid UniqueId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [MaxLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Short name is required")]
        [Display(Name = "Short name")]
        [MaxLength(20, ErrorMessage = "Short name cannot be longer than 20 characters")]
        public string ShortName { get; set; }

        public Guid? ParentUniqueId { get; set; }

        public bool IsRootUnit { get; set; }

        public bool Equals(StructureUnit other)
        {
            if (other == null)
            {
                return false;
            }

            return (UniqueId == other.UniqueId) &&
                   ((ParentUniqueId ?? Guid.Empty) == (other.ParentStructureUnit?.UniqueId ?? Guid.Empty)) &&
                   (Name == other.Name) && (ShortName == other.ShortName);
        }

        public override string ToString()
        {
            return String.Format("UniqueId: {0} Name:{1} ShortName:{2}", UniqueId, Name, ShortName);
        }
    }
}