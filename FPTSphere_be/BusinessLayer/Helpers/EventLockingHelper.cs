using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Constants;

namespace BusinessLayer.Helpers
{
    public static class EventLockingHelper
    {
        public static readonly HashSet<int> LockingStatusIds = new()
        {
            EventStatusIds.PendingApproval,
            EventStatusIds.Approved,
            EventStatusIds.InProgress
        };

        public static bool IsLocking(int statusId) => LockingStatusIds.Contains(statusId);
    }
}
