namespace Dilbert.Service.Tests
{
    using System.IO;
    using NUnit.Framework;
    using Services;

    [TestFixture]
    public class DilbertTests
    {
        private IDailyDilbertService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new DailyDilbertService();
        }

        [Test]
        public void given_an_instance_when_request_daily_as_file_then_file_path_is_returned()
        {
            // ARRANGE
            // ACT
            var task = _service.DailyAsFileAsync();
            task.Wait();

            var filePath = task.Result;

            // ASSERT
            Assert.That(filePath, Is.Not.Empty);
            Assert.That(new FileInfo(filePath).Exists, Is.True);
        }

        [Test]
        public void given_an_instance_when_request_daily_as_stream_then_file_stream_is_returned()
        {
            // ARRANGE
            // ACT
            var task = _service.DailyAsStreamAsync();
            task.Wait();

            var length = task.Result.Length;

            // ASSERT
            Assert.That(length, Is.Not.EqualTo(0));
        }
    }
}
