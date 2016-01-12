using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace skwas.IO.Tests
{
	[TestClass]
	public class UnmanagedStreamTests
	{
		#region Test helpers

		private const int SeekError = unchecked((int) 0x80030019);

		private static byte[] TestData = File.ReadAllBytes(typeof (RegionStreamTests).Assembly.Location);

		[DllImport("ole32")]
		private static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

		[DllImport("shlwapi")]
		private static extern IStream SHCreateMemStream(IntPtr pInit, uint cbInit);


		/// <summary>
		/// Creates a native IStream and writes data to it, and returns the pointer.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private static IStream CreateStreamOnHGlobal(byte[] data = null)
		{
			var hStream = data == null || data.Length == 0 ? IntPtr.Zero : Marshal.AllocHGlobal(data.Length);
			if (data != null && data.Length > 0) Marshal.Copy(data, 0, hStream, data.Length);

			IStream stream;
			int hResult;
			if ((hResult = CreateStreamOnHGlobal(hStream, true, out stream)) == 0) return stream;
			throw new COMException("Unable to create stream.", hResult);
		}


		/// <summary>
		/// Creates a native IStream, and returns the pointer.
		/// </summary>
		/// <returns></returns>
		private static IStream SHCreateMemStream()
		{
			return SHCreateMemStream(IntPtr.Zero, 0);
		}

		private UnmanagedStream test_access_modes(int grfMode, bool canSeek)
		{
			var mockStream = new Mock<IStream>();

			ComTypes.STATSTG pstatstg = new ComTypes.STATSTG
			{
				grfMode = grfMode
			};
			int grfStatFlag = 1;

			mockStream.Setup(s => s.Stat(out pstatstg, grfStatFlag)).Verifiable();
			if (!canSeek)
				mockStream.Setup(s => s.Seek(0, (int) SeekOrigin.Current, IntPtr.Zero)).Throws(new COMException("seek error", SeekError)).Verifiable();

			var ms = new UnmanagedStream(mockStream.Object);

			mockStream.Verify();

			return ms;
		}

		#endregion

		[TestMethod]
		public void it_will_open_empty_stream()
		{
			using (var ms = new UnmanagedStream(CreateStreamOnHGlobal()))
			{
				ms.CanRead.Should().BeTrue("because it should support reading");
				ms.CanWrite.Should().BeTrue("because it should support writing");
				ms.CanSeek.Should().BeTrue("because it should support seeking");
				ms.Position.Should().Be(0, "because the stream is at the start");
				ms.Length.Should().Be(0, "because the stream is empty");
			}
		}

		[TestMethod]
		public void it_will_write_to_stream_and_read_back()
		{
			var halfTestDataSize = TestData.Length/2;

			using (var ms = new UnmanagedStream(SHCreateMemStream()))
			{
				ms.CanWrite.Should().BeTrue("because it should support writing");

				ms.Write(TestData, 0, TestData.Length);

				ms.Position.Should().NotBe(0, "because the stream has advanced").And.Be(ms.Length);

				ms.Position -= ms.Position;
				ms.Position.Should().Be(0, "because we seeked back to the start");

				var readBuffer = new byte[TestData.Length];
				ms.Read(readBuffer, 0, readBuffer.Length);

				readBuffer.ShouldBeEquivalentTo(TestData);
			}

			using (var ms = new UnmanagedStream(SHCreateMemStream()))
			{
				// Write two blocks.
				ms.Write(TestData, 0, halfTestDataSize);
				ms.Write(TestData, halfTestDataSize, halfTestDataSize);

				ms.Position -= ms.Position;

				// Read two blocks.
				var readBuffer = new byte[TestData.Length];
				ms.Read(readBuffer, 0, halfTestDataSize);
				ms.Read(readBuffer, halfTestDataSize, halfTestDataSize);

				readBuffer.ShouldBeEquivalentTo(TestData);

				// Until eof.
				while (ms.Read(readBuffer, 0, readBuffer.Length) != 0)
				{
				}
			}
		}

		[TestMethod]
		public void determines_access_mode_read()
		{
			using (var ms = test_access_modes(0, true))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeFalse();
				ms.CanSeek.Should().BeTrue();
			}
			using (var ms = test_access_modes(0, false))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeFalse();
				ms.CanSeek.Should().BeFalse();
			}
		}

		[TestMethod]
		public void determines_access_mode_write()
		{
			using (var ms = test_access_modes(1, true))
			{
				ms.CanRead.Should().BeFalse();
				ms.CanWrite.Should().BeTrue();
				ms.CanSeek.Should().BeTrue();
			}
			using (var ms = test_access_modes(1, false))
			{
				ms.CanRead.Should().BeFalse();
				ms.CanWrite.Should().BeTrue();
				ms.CanSeek.Should().BeFalse();
			}
		}

		[TestMethod]
		public void determines_access_mode_read_write()
		{
			using (var ms = test_access_modes(2, true))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeTrue();
				ms.CanSeek.Should().BeTrue();
			}
			using (var ms = test_access_modes(2, false))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeTrue();
				ms.CanSeek.Should().BeFalse();
			}
		}

		[TestMethod]
		public void when_created_with_auto_close_dispose_releases_stream()
		{
			// Create stream and put some data in it.
			var stream = SHCreateMemStream();
			using (var ms = new UnmanagedStream(stream))
				ms.Write(new byte[] {0, 1, 2, 3}, 0, 4);

			// Test if IStream is released by reading using its methods.
			Action action = () => stream.Seek(0, 0, IntPtr.Zero);
			action.ShouldThrow<InvalidComObjectException>();
		}

		[TestMethod]
		public void when_created_with_leave_open_dispose_does_not_release_stream()
		{
			// Create stream and put some data in it.
			var stream = SHCreateMemStream();
			using (var ms = new UnmanagedStream(stream, true))
				ms.Write(new byte[] {0, 1, 2, 3}, 0, 4);

			// Test if IStream is not released by reading using its methods.
			stream.Seek(0, 0, IntPtr.Zero);

			// Manual release and test again.
			Marshal.ReleaseComObject(stream);
			Action action = () => stream.Seek(0, 0, IntPtr.Zero);
			action.ShouldThrow<InvalidComObjectException>();
		}

		[TestMethod]
		public void when_accessing_members_after_disposed_throws_up()
		{
			var ms = new UnmanagedStream(SHCreateMemStream(), true);
			ms.Dispose();

			Action action;

			action = () => ms.Flush();
			action.ShouldThrow<ObjectDisposedException>();

			action = () => new Func<long>(() => ms.Length).Invoke();
			action.ShouldThrow<ObjectDisposedException>();

			action = () => new Func<long>(() => ms.Position).Invoke();
			action.ShouldThrow<ObjectDisposedException>();

			action = () => ms.ReadByte();
			action.ShouldThrow<ObjectDisposedException>();

			action = () => ms.WriteByte(0);
			action.ShouldThrow<ObjectDisposedException>();

			action = () => ms.SetLength(0);
			action.ShouldThrow<ObjectDisposedException>();
		}

		[TestMethod]
		public void when_accessing_members_after_underlying_stream_released_should_throw()
		{
			var buf = new byte[1024];
			var stream = SHCreateMemStream();
			var ms = new UnmanagedStream(stream);
			Marshal.ReleaseComObject(stream);

			Action action;

			action = () => new Func<long>(() => ms.Length).Invoke();
			action.ShouldThrow<InvalidOperationException>();

			action = () => new Func<long>(() => ms.Position).Invoke();
			action.ShouldThrow<InvalidOperationException>();

			action = () => ms.Read(buf, 0, buf.Length);
			action.ShouldThrow<InvalidOperationException>();

			action = () => ms.Write(buf, 0, buf.Length);
			action.ShouldThrow<InvalidOperationException>();

			action = () => ms.SetLength(0);
			action.ShouldThrow<InvalidOperationException>();
		}

		[TestMethod]
		public void when_disposing_more_than_once_nothing_happens()
		{
			var ms = new UnmanagedStream(SHCreateMemStream(), true);
			ms.Dispose();

			Action action = () => ms.Dispose();
			action.ShouldNotThrow<ObjectDisposedException>();
		}

		[TestMethod]
		public void when_creating_with_released_stream_should_throw()
		{
			var stream = SHCreateMemStream();
			Marshal.ReleaseComObject(stream);

			Action action = () => new UnmanagedStream(stream);
			action.ShouldThrow<IOException>();
		}

		[TestMethod]
		public void when_creating_with_null_stream_should_throw()
		{
			Action action = () => new UnmanagedStream(null);
			action.ShouldThrow<ArgumentNullException>();
		}

		[TestMethod]
		public void when_reading_from_writeonly_stream_should_throw()
		{
			using (var ms = test_access_modes(1, true))
			{
				Action action = () => ms.ReadByte();
				action.ShouldThrow<NotSupportedException>();
			}
		}

		[TestMethod]
		public void when_writing_on_readonly_stream_should_throw()
		{
			using (var ms = test_access_modes(0, true))
			{
				Action action = () => ms.Write(new byte[0], 0, 0);
				action.ShouldThrow<NotSupportedException>();
			}
		}

		[TestMethod]
		public void when_seeking_on_non_seekable__stream_should_throw()
		{
			using (var ms = test_access_modes(0, false))
			{
				Action action = () => ms.Position += 10;
				action.ShouldThrow<NotSupportedException>();
			}
		}

		[TestMethod]
		public void when_reading_with_invalid_arguments_should_throw()
		{
			using (var ms = test_access_modes(0, true))
			{
				var buffer = new byte[2048];
				Action action;

				action = () => ms.Read(null, 0, 0);
				action.ShouldThrow<ArgumentNullException>();

				action = () => ms.Read(buffer, -1, 0);
				action.ShouldThrow<ArgumentOutOfRangeException>();

				action = () => ms.Read(buffer, buffer.Length*2, 0);
				action.ShouldThrow<ArgumentOutOfRangeException>();

				action = () => ms.Read(buffer, 0, -1);
				action.ShouldThrow<ArgumentOutOfRangeException>();

				action = () => ms.Read(buffer, 0, buffer.Length*2);
				action.ShouldThrow<ArgumentOutOfRangeException>();

				action = () => ms.Read(buffer, buffer.Length, buffer.Length);
				action.ShouldThrow<ArgumentOutOfRangeException>();
			}
		}

		[TestMethod]
		public void when_setting_length_on_readonly_or_unseekable_stream_should_throw()
		{
			const long size = 1000;

			// Read, can't seek.
			using (var ms = test_access_modes(0, false))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldThrow<NotSupportedException>();
			}

			// Read, can seek.
			using (var ms = test_access_modes(0, true))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldThrow<NotSupportedException>();
			}

			// Write, can't seek.
			using (var ms = test_access_modes(1, false))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldThrow<NotSupportedException>();
			}

			// Write, can seek.
			using (var ms = test_access_modes(1, true))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldNotThrow<NotSupportedException>();
			}

			// Read/write, can't seek.
			using (var ms = test_access_modes(2, false))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldThrow<NotSupportedException>();
			}

			// Read/write, can seek.
			using (var ms = test_access_modes(2, true))
			{
				Action action = () => ms.SetLength(size);
				action.ShouldNotThrow<NotSupportedException>();
			}
		}

		[TestMethod]
		public void when_setting_length_on_stream_should_resize()
		{
			using (var ms = new UnmanagedStream(CreateStreamOnHGlobal()))
			{
				ms.Length.Should().Be(0);

				ms.SetLength(10000);
				ms.Length.Should().Be(10000);

				ms.SetLength(5000);
				ms.Length.Should().Be(5000);
			}
		}


	}
}
