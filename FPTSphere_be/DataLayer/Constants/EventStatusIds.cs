using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Constants
{
    public static class EventStatusIds
    {
        public const int Draft = 1;
        public const int PendingApproval = 2;
        public const int Approved = 3;
        public const int InProgress = 4;
        public const int Completed = 5;
        public const int Cancelled = 6;
        public const int Rejected = 7;
    }
}
