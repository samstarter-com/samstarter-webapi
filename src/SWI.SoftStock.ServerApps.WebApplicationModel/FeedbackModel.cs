using System.ComponentModel.DataAnnotations;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class FeedbackModel
    {
        public string Title { get; set; }
        
        public string Email { get; set; }

        public string Comment { get; set; }
    }
}