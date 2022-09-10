using JWTAuth.DataAccess.Models;

namespace JWTAuth.Test.Server
{
    [TestClass]
    internal class GlobalTestSettings
    {
        private const string CONNECTION_STRING_TEST = @"Server=(localdb)\mssqllocaldb;Database=RefreshAuthDB1Test;Trusted_Connection=True;MultipleActiveResultSets=true";
        internal static DbScope Db { get; private set; }

        [AssemblyInitialize]
        public static async Task GlobalTestSetup(TestContext test)
        {
            Db = DbScope.CreateContext(CONNECTION_STRING_TEST);
            await Db.Database.EnsureCreatedAsync();
        }

        [AssemblyCleanup]
        public static async Task GlobalTearDown()
        {
            await Db.Database.EnsureDeletedAsync();
            await Db.DisposeAsync();
        }
    }
}
