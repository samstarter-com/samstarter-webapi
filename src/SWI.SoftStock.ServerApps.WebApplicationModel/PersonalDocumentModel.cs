using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class PersonalDocumentModel
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public Guid Id { get; set; }
    }
}