using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class UploadedDocumentModel
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public Guid UploadId { get; set; }
    }
}