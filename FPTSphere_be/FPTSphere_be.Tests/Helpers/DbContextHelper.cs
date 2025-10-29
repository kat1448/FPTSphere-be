using DataLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTSphere_be.Tests.Helpers
{
    public static class DbContextHelper
    {
        public static EventDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            return new EventDbContext(options);
        }
    }
}