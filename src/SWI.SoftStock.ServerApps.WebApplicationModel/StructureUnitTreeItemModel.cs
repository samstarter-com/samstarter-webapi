using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;
    using System.Collections.Generic;


    [JsonObject]
    public class StructureUnitTreeItemState
    {
        private bool isOpen;

        private bool isSelected;

        private StructureUnitTreeItemModel parent;

        public StructureUnitTreeItemState(StructureUnitTreeItemModel parent)

        {
            this.parent = parent;
        }


        [JsonProperty("opened")]
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
            private set
            {
                isOpen = value;
                if (isOpen && parent != null)
                {
                    parent.State.IsOpen = true;
                }
            }
        }

        [JsonProperty("disabled")]
        public bool IsDisabled
        {
            get
            {
                return false;
            }
        }
        [JsonProperty("selected")]
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                if (isSelected && parent != null)
                {
                    parent.State.IsOpen = true;
                }
            }
        }
    }

    public class StructureUnitTreeItemModel
    {       
        [JsonProperty("id")]
        public Guid UniqueId { get; set; }
       
        [JsonProperty("text")]
        public string ShortName { get; set; }

        [JsonProperty("state")]
        public StructureUnitTreeItemState State { get; set; }


        [JsonProperty("children")]
        public IEnumerable<StructureUnitTreeItemModel> ChildUnits { get; set; }

        public StructureUnitTreeItemModel Parent { private get; set; }
        
        public override string ToString()
        {
            return String.Format("UniqueId: {0} ShortName:{1}", UniqueId,  ShortName);
        }
    }
}