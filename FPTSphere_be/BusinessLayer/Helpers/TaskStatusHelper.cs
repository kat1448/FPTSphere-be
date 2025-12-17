using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Helpers
{
    public static class TaskStatusHelper
    {
        public const string ToDo = "To Do";
        public const string InProgress = "In Progress";
        public const string Completed = "Completed";

        public static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ToDo;

            var s = input.Trim().ToLowerInvariant();

            // Accept synonyms
            if (s is "todo" or "to do" or "to-do" or "draft" or "pending" or "new")
                return ToDo;

            if (s is "inprogress" or "in progress" or "doing" or "processing" or "working")
                return InProgress;

            if (s is "done" or "complete" or "completed" or "finished")
                return Completed;

            throw new InvalidOperationException($"Invalid status. Allowed: '{ToDo}', '{InProgress}', '{Completed}'.");
        }

        public static bool IsCompleted(string status) =>
            string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase);
    }
}
