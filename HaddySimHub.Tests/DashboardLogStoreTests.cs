using HaddySimHub.Dashboard;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class DashboardLogStoreTests
    {
        private const int Capacity = 500;

        private static LogEntry Entry(int index) =>
            new(new DateTime(2024, 1, 1).AddSeconds(index), "INFO", $"message-{index}");

        [TestMethod]
        public void Add_ThenSnapshot_ReturnsEntry()
        {
            var store = new DashboardLogStore();
            var entry = Entry(1);

            store.Add(entry);

            var snapshot = store.Snapshot(10);
            Assert.AreEqual(1, snapshot.Count);
            Assert.AreSame(entry, snapshot[0]);
        }

        [TestMethod]
        public void Add_Null_ThrowsArgumentNullException()
        {
            var store = new DashboardLogStore();

            Assert.ThrowsExactly<ArgumentNullException>(() => store.Add(null!));
        }

        [TestMethod]
        public void Add_BeyondCapacity_DropsOldestAndCapsCount()
        {
            var store = new DashboardLogStore();
            const int total = Capacity + 100;

            for (var i = 0; i < total; i++)
            {
                store.Add(Entry(i));
            }

            var snapshot = store.Snapshot(0);
            Assert.AreEqual(Capacity, snapshot.Count);

            // The first 100 entries should have been evicted (FIFO).
            Assert.AreEqual("message-100", snapshot[0].Message);
            Assert.AreEqual($"message-{total - 1}", snapshot[^1].Message);
        }

        [TestMethod]
        public void Snapshot_ReturnsMostRecentInOrder()
        {
            var store = new DashboardLogStore();
            for (var i = 0; i < 10; i++)
            {
                store.Add(Entry(i));
            }

            var snapshot = store.Snapshot(3);

            Assert.AreEqual(3, snapshot.Count);
            Assert.AreEqual("message-7", snapshot[0].Message);
            Assert.AreEqual("message-8", snapshot[1].Message);
            Assert.AreEqual("message-9", snapshot[2].Message);
        }

        [TestMethod]
        public void Snapshot_WithNonPositiveMax_ReturnsAllEntries()
        {
            var store = new DashboardLogStore();
            for (var i = 0; i < 5; i++)
            {
                store.Add(Entry(i));
            }

            Assert.AreEqual(5, store.Snapshot(0).Count);
            Assert.AreEqual(5, store.Snapshot(-1).Count);
        }

        [TestMethod]
        public void Snapshot_WhenEmpty_ReturnsEmpty()
        {
            var store = new DashboardLogStore();

            Assert.AreEqual(0, store.Snapshot(10).Count);
        }
    }
}
