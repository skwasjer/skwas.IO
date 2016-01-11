using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace skwas.IO.Tests
{
	[TestClass]
	public class ProgressStreamTests
	{
		private class ProgressReporter
			: IProgress<ProgressStream.Progress>
		{
			public int CallbackCount;
			public float LastPercentage;

			#region Implementation of IProgress<in Progress>

			/// <summary>
			/// Reports a progress update.
			/// </summary>
			/// <param name="value">The value of the updated progress.</param>
			public void Report(ProgressStream.Progress value)
			{
				CallbackCount++;
				LastPercentage = value.Percentage;
			}

			#endregion
		}

		private static byte[] CreateStreamMock(out Mock<Stream> mockStream)
		{
			var buffer = new byte[10];
			var streamSize = 1000000;
			var pos = 0;
			mockStream = new Mock<Stream>();
			mockStream.Setup(s => s.Length).Returns(() => streamSize).Verifiable();
			mockStream.Setup(s => s.Position).Returns(() => pos).Verifiable();
			mockStream
				.Setup(s => s.Read(buffer, 0, buffer.Length))
				.Callback(() =>
				{
					// Advance mocked stream forward.
					pos = Math.Min(pos + buffer.Length, streamSize);
				})
				.Returns(() => pos >= streamSize ? 0 : buffer.Length)
				.Verifiable();
			return buffer;
		}

		[TestMethod]
		public void reports_progress_via_action()
		{
			var callbackCount = 0;
			var lastPercentage = 0f;

			Action<ProgressStream.Progress> callback = progress =>
			{
				callbackCount++;
				lastPercentage = progress.Percentage;
			};

			Mock<Stream> mockStream;
			var buffer = CreateStreamMock(out mockStream);

			using (var progressStream = new ProgressStream(mockStream.Object, callback))
			{
				callbackCount.Should().Be(0);
				lastPercentage.Should().Be(0);

				while (true)
				{
					var bytesRead = progressStream.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0) break;
				}
			}

			mockStream.Verify();

			callbackCount.Should().BeGreaterThan(0);
			lastPercentage.Should().Be(100f);
		}

		[TestMethod]
		public void reports_progress_via_progress_t()
		{
			var callback = new ProgressReporter();

			Mock<Stream> mockStream;
			var buffer = CreateStreamMock(out mockStream);

			using (var progressStream = new ProgressStream(mockStream.Object, callback))
			{
				callback.CallbackCount.Should().Be(0);
				callback.LastPercentage.Should().Be(0);

				while (true)
				{
					var bytesRead = progressStream.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0) break;
				}
			}

			mockStream.Verify();

			callback.CallbackCount.Should().BeGreaterThan(0);
			callback.LastPercentage.Should().Be(100f);
		}
	}
}
