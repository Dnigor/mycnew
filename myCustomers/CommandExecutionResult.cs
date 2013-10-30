using System.Collections.Generic;

namespace myCustomers
{
    public class CommandExecutionResult
    {
        private readonly List<string> _errors;

        public CommandExecutionResult()
        {
            IsSuccess = false;
            _errors = new List<string>();
        }

        public bool IsSuccess { get; set; }

        public IEnumerable<string> Errors
        {
            get { return _errors; }
        }

        public void AddErrorMessages(params string[] messages)
        {
            if (messages == null)
                return;

            _errors.AddRange(messages);
            IsSuccess = false;
        }
    }
}