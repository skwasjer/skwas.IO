using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;

namespace skwas.IO
{
	/// <summary>
	/// Encapsulates an unmanaged IStream to provide access from managed code.
	/// </summary>
	/// <remarks>Other implementations are not as complete as my current implementation (as far as I have found), and do not support for instance, reading to or writing from an offset in a byte array, or assume each IStream is always both readable and writeable, or are required to be compiled using unsafe context (for using unchecked pointers).</remarks>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public sealed class UnmanagedStream : Stream
	{
		private bool _disposed;
		private readonly bool _releaseOnDispose;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IStream _stream;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IntPtr _hBytesRead;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IntPtr _hPosition;

		#region .ctor/~() / IDisposable

		/// <summary>
		/// Initializes a new instance of <see cref="UnmanagedStream"/> using the specified <see cref="IStream"/>. 
		/// </summary>
		/// <param name="stream">The <see cref="IStream"/> to encapsulate.</param>
		/// <param name="releaseOnDispose">When true, <see cref="Marshal.ReleaseComObject"/> will be called on the stream.</param>
		[SecuritySafeCritical]
		public UnmanagedStream(IStream stream, bool releaseOnDispose = true)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			// Mask with only Read/Write/ReadWrite.
			var accessMode = GetAccessMode(stream) & (NativeMethods.Stgm.Read | NativeMethods.Stgm.Write | NativeMethods.Stgm.ReadWrite);

			CanRead = accessMode == NativeMethods.Stgm.Read || accessMode == NativeMethods.Stgm.ReadWrite;
			CanWrite = accessMode == NativeMethods.Stgm.Write || accessMode == NativeMethods.Stgm.ReadWrite;

			// Check if the stream is seekable.
			try
			{
				stream.Seek(0, (int)SeekOrigin.Current, IntPtr.Zero);
				CanSeek = true;
			}
			catch (Exception)
			{
				// Ignore
			}

			// Set up two pointers used by the class for retrieving info from the underlying IStream.
			_hBytesRead = Marshal.AllocCoTaskMem(4);	// Pointer to an Int32.

			try
			{
				_hPosition = Marshal.AllocCoTaskMem(8);		// Pointer to an Int64.
			}
			catch (OutOfMemoryException)
			{
				// If this has failed, we need to clean up the previous memory initialization call, as the object will be invalid.
				Marshal.FreeCoTaskMem(_hBytesRead);
				_hBytesRead = IntPtr.Zero;
				
				throw;	// And now, rethrow.
			}

			_stream = stream;
			_releaseOnDispose = releaseOnDispose;
		}

		/// <summary>
		/// Releases the unmanaged resources used by <see cref="UnmanagedStream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"></param>
		[SecuritySafeCritical]
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_disposed) return;

			if (disposing)
			{
				// Release managed.
				Flush();

				if (_stream != null && Marshal.IsComObject(_stream) && _releaseOnDispose) Marshal.ReleaseComObject(_stream);
			}

			// Release unmanaged.			
			if (_hBytesRead != IntPtr.Zero) Marshal.FreeCoTaskMem(_hBytesRead);
			if (_hPosition != IntPtr.Zero) Marshal.FreeCoTaskMem(_hPosition);

			_disposed = true;
		}

		#endregion

		#region Stream overrides

		/// <summary>
		/// Gets a value indicating whether a stream supports reading.
		/// </summary>
		public override bool CanRead { get; }


		/// <summary>
		/// Gets a value indicating whether a stream supports seeking.
		/// </summary>
		public override bool CanSeek { get; }


		/// <summary>
		/// Gets a value indicating whether a stream supports writing.
		/// </summary>
		public override bool CanWrite { get; }

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		[SecuritySafeCritical]
		public override void Flush()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);

			try
			{
				_stream.Commit((int)NativeMethods.STGC.Default);
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamCantFlush, ex);
			}
		}


		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		public override long Length
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);

				try
				{
					ComTypes.STATSTG stat;
					_stream.Stat(out stat, (int)NativeMethods.STATFLAG.NoName);
					return stat.cbSize;
				}
				catch (Exception ex)
				{
					throw new IOException(Resources.UnmanagedStream.IOException_StreamGetLength, ex);
				}
			}
		}


		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		public override long Position
		{
			get
			{
				return Seek(0, SeekOrigin.Current);
			}
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}


		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		[SecuritySafeCritical]
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset + count > buffer.Length)
				throw new ArgumentException();
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (!CanRead)
				throw new NotSupportedException(Resources.UnmanagedStream.Argument_StreamNotReadable);

			// We are at end of stream?
			if (Position >= Length)
				return 0;

			int bytesRead;
			try
			{
				if (offset == 0)
				{
					_stream.Read(buffer, count, _hBytesRead);
					bytesRead = Marshal.ReadInt32(_hBytesRead);
				}
				else
				{
					var b = new byte[count];
					_stream.Read(b, count, _hBytesRead);
					bytesRead = Marshal.ReadInt32(_hBytesRead);
					Buffer.BlockCopy(b, 0, buffer, offset, bytesRead);
				}
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamRead, ex);
			}
			return bytesRead;
		}


		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
			if (!CanSeek)
				throw new NotSupportedException(Resources.UnmanagedStream.Argument_StreamNotSeekable);

			try
			{
				_stream.Seek(offset, (int)origin, _hPosition);
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamSeek, ex);
			}
			return Marshal.ReadInt64(_hPosition);
		}


		/// <summary>
		/// Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		[SecuritySafeCritical]
		public override void SetLength(long value)
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
			if (!CanSeek)
				throw new NotSupportedException(Resources.UnmanagedStream.Argument_StreamNotSeekable);
			if (!CanWrite)
				throw new NotSupportedException(Resources.UnmanagedStream.Argument_StreamNotWriteable);

			try
			{
				_stream.SetSize(value);
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamSetLength, ex);
			}
		}


		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		[SecuritySafeCritical]
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset + count > buffer.Length)
				throw new ArgumentException();
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (!CanWrite)
				throw new NotSupportedException(Resources.UnmanagedStream.Argument_StreamNotWriteable);

			try
			{
				// Copy buffer directly.
				if (offset == 0)
				{
					_stream.Write(buffer, count, IntPtr.Zero);
				}
				else
				{
					// Create a local temp buffer, and copy the data. We need the buffer to have an offset of 0 because IStream does not support writing with an offset.
					var b = new byte[count];
					Buffer.BlockCopy(buffer, offset, b, 0, count);
					_stream.Write(b, count, IntPtr.Zero);
				}
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamWrite, ex);
			}
		}

		#endregion

		#region Helpers

		[SecuritySafeCritical]
		private static NativeMethods.Stgm GetAccessMode(IStream stream)
		{
			try
			{
				// If IStream is created via CreateStreamOnHGlobal, stream.Stat() does not return the proper read/write state of the stream.
				// We bypass this by checking if it is created that way, and just return ReadWrite mode.
				IntPtr hStream;
				if (0 == NativeMethods.GetHGlobalFromStream(stream, out hStream) && hStream != IntPtr.Zero)
					return NativeMethods.Stgm.ReadWrite;

				// Get statistics from IStream.
				ComTypes.STATSTG stat;
				stream.Stat(out stat, (int)NativeMethods.STATFLAG.NoName);
				return (NativeMethods.Stgm)stat.grfMode;
			}
			catch (Exception ex)
			{
				throw new IOException(Resources.UnmanagedStream.IOException_StreamNotInitialized, ex);
			}
		}

		#endregion
	}
}
