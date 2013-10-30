using System;

namespace myCustomers.Web.Models
{
    public class NoteContent
    {
        public string Content { get; set; }
        public Guid CustomerNoteKey { get; set; }
        public DateTime DateCreatedUtc { get; set; }
    }
}