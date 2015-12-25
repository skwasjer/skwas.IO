﻿using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using skwas.IO;

namespace skwas.IO_Tests
{
	[TestClass]
	public class RegionStreamTests
	{
		private static Stream GetTestStream(long seekToPosition = 0)
		{
			var ms = new MemoryStream(File.ReadAllBytes(typeof (RegionStreamTests).Assembly.Location))
			{
				Position = seekToPosition
			};
			return ms;
		}

		[TestMethod]
		public void stream_starts_at_start()
		{
			const long length = 200L;

			using (var s = new RegionStream(GetTestStream(), length))
			{
				s.Position.Should().Be(0, "because a region stream starts at 0");
				s.BaseStream.Position.Should().Be(0, "because the base stream started at 0");
				s.Length.Should().Be(length, "because a region stream has a fixed length of {0}", length);
			}
		}

		[TestMethod]
		public void stream_starts_in_middle()
		{
			const long position = 100L;
			const long length = 200L;

			using (var s = new RegionStream(GetTestStream(position), length))
			{
				s.Position.Should().Be(0, "because a region stream starts at 0");
				s.BaseStream.Position.Should().Be(position, "because a region stream starts at {0}", position);
				s.Length.Should().Be(length, "because a region stream has a fixed length of {0}", length);
			}
		}

		[TestMethod]
		public void stream_can_seek_from_begin()
		{
			var testStream = GetTestStream();
			var origin = testStream.Position = testStream.Length/2;

			Action action;

			const long length = 500L;
			using (var s = new RegionStream(testStream, length))
			{
				s.Position.Should().Be(0, "because a region stream starts at 0");

				action = () => s.Seek(-10, SeekOrigin.Begin);
				action.ShouldThrow<IOException>("because offset is less then 0");
				s.Position.Should().Be(0, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin);

				s.Seek(50, SeekOrigin.Begin);
				s.Position.Should().Be(50);
				s.BaseStream.Position.Should().Be(origin + 50);

				s.Seek(100, SeekOrigin.Begin);
				s.Position.Should().Be(100);
				s.BaseStream.Position.Should().Be(origin + 100);

				action = () => s.Seek(length + 50, SeekOrigin.Begin);
				action.ShouldThrow<IOException>("because offset exceeds end of stream");
				s.Position.Should().Be(100, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin + 100);
			}
		}

		[TestMethod]
		public void stream_can_seek_from_current()
		{
			var testStream = GetTestStream();
			var origin = testStream.Position = testStream.Length / 2;

			Action action;

			const long length = 500L;
			using (var s = new RegionStream(testStream, length))
			{
				s.Position.Should().Be(0, "because a region stream starts at 0");

				action = () => s.Seek(-10, SeekOrigin.Current);
				action.ShouldThrow<IOException>("because new position is less then 0");
				s.Position.Should().Be(0, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin);

				s.Seek(50, SeekOrigin.Current);
				s.Position.Should().Be(50);
				s.BaseStream.Position.Should().Be(origin + 50);

				s.Seek(70, SeekOrigin.Current);
				s.Position.Should().Be(120);
				s.BaseStream.Position.Should().Be(origin + 120);

				s.Seek(-20, SeekOrigin.Current);
				s.Position.Should().Be(100);
				s.BaseStream.Position.Should().Be(origin + 100);

				action = () => s.Seek(length + 50, SeekOrigin.Current);
				action.ShouldThrow<IOException>("because new position exceeds end of stream");
				s.Position.Should().Be(100, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin + 100);
			}
		}

		[TestMethod]
		public void stream_can_seek_from_end()
		{
			var testStream = GetTestStream();
			var origin = testStream.Position = testStream.Length / 2;

			Action action;

			const long length = 500L;
			using (var s = new RegionStream(testStream, length))
			{
				s.Position.Should().Be(0, "because a region stream starts at 0");

				action = () => s.Seek(-10, SeekOrigin.End);
				action.ShouldThrow<IOException>("because offset is less then 0");
				s.Position.Should().Be(0, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin);

				s.Seek(50, SeekOrigin.End);
				s.Position.Should().Be(length - 50);
				s.BaseStream.Position.Should().Be(origin + length - 50);

				action = () => s.Seek(length + 50, SeekOrigin.End);
				action.ShouldThrow<IOException>("because offset exceeds start of stream");
				s.Position.Should().Be(length - 50, "because position should not have changed");
				s.BaseStream.Position.Should().Be(origin + length - 50);
			}
		}
	}
}
