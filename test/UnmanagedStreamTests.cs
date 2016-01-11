using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace skwas.IO.Tests
{
	[TestClass]
	public class UnmanagedStreamTests
	{
		[DllImport("ole32")]
		static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

		[DllImport("shlwapi")]
		static extern IStream SHCreateMemStream(IntPtr pInit, uint cbInit);


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
			using (var ms = new UnmanagedStream(SHCreateMemStream()))
			{
				ms.CanWrite.Should().BeTrue("because it should support writing");

				var buffer = File.ReadAllBytes(GetType().Assembly.Location);
				ms.Write(buffer, 0, buffer.Length);

				ms.Position.Should().NotBe(0, "because the stream has advanced").And.Be(ms.Length);

				ms.Seek(-ms.Position, SeekOrigin.Current);
				ms.Position.Should().Be(0, "because we seeked back to the start");

				var readBuffer = new byte[ms.Length];
				ms.Read(readBuffer, 0, readBuffer.Length);

				readBuffer.Should().BeEquivalentTo(buffer);
			}
		}

		private UnmanagedStream test_access_modes(int grfMode)
		{
			var mockStream = new Mock<IStream>();

			ComTypes.STATSTG pstatstg = new ComTypes.STATSTG
			{
				grfMode = grfMode
			};
			int grfStatFlag = 1;

			mockStream.Setup(s => s.Stat(out pstatstg, grfStatFlag));

			var ms = new UnmanagedStream(mockStream.Object);

			mockStream.Verify(s => s.Stat(out pstatstg, grfStatFlag));

			return ms;
		}

		[TestMethod]
		public void determines_access_mode_read()
		{
			using (var ms = test_access_modes(0))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeFalse();
			}
		}

		[TestMethod]
		public void determines_access_mode_write()
		{
			using (var ms = test_access_modes(1))
			{
				ms.CanRead.Should().BeFalse();
				ms.CanWrite.Should().BeTrue();
			}
		}

		[TestMethod]
		public void determines_access_mode_read_write()
		{
			using (var ms = test_access_modes(2))
			{
				ms.CanRead.Should().BeTrue();
				ms.CanWrite.Should().BeTrue();
			}
		}

		[TestMethod]
		public void when_created_with_auto_close_dispose_releases_stream()
		{
			// Create stream and put some data in it.
			var stream = SHCreateMemStream();
			using (var ms = new UnmanagedStream(stream))
				ms.Write(new byte[] { 0, 1, 2, 3 }, 0, 4);

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
				ms.Write(new byte[] { 0, 1, 2, 3 }, 0, 4);

			// Test if IStream is not released by reading using its methods.
			stream.Seek(0, 0, IntPtr.Zero);

			// Manual release and test again.
			Marshal.ReleaseComObject(stream);
			Action action = () => stream.Seek(0, 0, IntPtr.Zero);
			action.ShouldThrow<InvalidComObjectException>();
		}
	}
}
