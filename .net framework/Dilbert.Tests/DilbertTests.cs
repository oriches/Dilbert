﻿using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dilbert.Tests
{
    [TestFixture]
    public class DilbertTests
    {
        [SetUp]
        public void SetUp()
        {
            _service = new DailyDilbertService();
        }

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                var file = await _service.DailyAsFileAsync();
                File.Delete(file);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to tear down dilbert file!");
            }
        }

        private DailyDilbertService _service;

        [Test]
        public async Task given_an_instance_when_request_daily_as_file_then_file_path_is_returned()
        {
            // ARRANGE
            // ACT
            var filePath = await _service.DailyAsFileAsync();
            
            // ASSERT
            Assert.That(filePath, Is.Not.Empty);
            Assert.That(new FileInfo(filePath).Exists, Is.True);
        }

        [Test]
        public async Task given_an_instance_when_request_daily_as_stream_then_file_stream_is_returned()
        {
            // ARRANGE
            // ACT
            var filePath = await _service.DailyAsFileAsync();

            var length = filePath.Length;

            // ASSERT
            Assert.That(length, Is.Not.EqualTo(0));
        }
    }
}