using System;

namespace myCustomers
{
    public class CommandException : Exception
    {
        public string[] Errors { get; set; }
    }
}
