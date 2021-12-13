using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class DocumentModelEx
    {
        public string Name { get; set; }
        public string HcLocation { get; set; }
        public byte[] Content { get; set; }
        public Guid Id { get; set; }
        public Guid UploadId { get; set; }
    }
}