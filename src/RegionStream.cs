using System;
using System.IO;

namespace skwas.IO
{
	/// <summary>
	/// Wraps a stream into a specific isolated region.
	/// </summary>
	public class RegionStream : Stream
	{
		private bool _disposed;
		private readonly bool _forReading = true;
		private readonly Stream _baseStream;

		private long _origin;
		private long _count;

		/// <summary>
		/// Initializes a new instance of <see cref="RegionStream"/>. The specified stream will be wrapped and access is only allowed from the current stream position and with a fixed <paramref name="length"/>.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="length"></param>
		/// <param name="forReading"></param>
		public RegionStream(Stream stream, long length, bool forReading = true)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			_baseStream = stream;

			Move(stream.Position, length);

			_forReading = forReading;
/*			_startPosition = stream.Position;

			if (_forReading)
			{
				if (count + _startPosition > _baseStream.Length)
					throw new ArgumentOutOfRangeException("count");

			}
			_count = count;*/
		}

		/// <summary>
		/// Moves the stream region to the new start position and length. Note that this method can only be called if the stream is in 'read' mode. A stream in write mode, cannot be moved.
		/// </summary>
		/// <param name="startPosition"></param>
		/// <param name="length"></param>
		public void Move(long startPosition, long length)
		{
			if (!_forReading)
				throw new NotSupportedException("The stream does not support reading.");

			if (length + startPosition > _baseStream.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			_origin = startPosition;
			_count = length;

			// Move stream.
			_baseStream.Position = startPosition;
		}

		public Stream BaseStream => _baseStream;

		public override bool CanRead
		{
			get 
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				return _baseStream.CanRead && _forReading;  
			}
		}

		public override bool CanSeek
		{
			get 
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				return _baseStream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				return _baseStream.CanWrite && !_forReading;
			}
		}

		public override void Flush()
		{
			_baseStream.Flush();
		}

		public override long Length
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				if (_forReading)
					return _count;
				return _baseStream.Length - _origin;
			}
		}

		public override long Position
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				return _baseStream.Position - _origin;
			}
			set
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				Seek(value, SeekOrigin.Begin);
//				_baseStream.Position = _startPosition + value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset + count > buffer.Length)
				throw new ArgumentException();
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (!CanRead)
				throw new NotSupportedException();
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);

			// We are at end of stream?
			if (Position >= Length)
				return 0;

			// If requested bytes exceeds this stream, reduce number of bytes to read from base stream.
			if (Position + count > Length)
				count = (int)(Length - Position);

			return _baseStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long newPosition;
			switch (origin)
			{
				case SeekOrigin.Begin:				
					newPosition = _origin + offset;
					if (offset < 0 || newPosition < _origin)
						throw new IOException("IO.IO_SeekBeforeBegin");
					if (newPosition > _origin + _count)
						throw new IOException("IO.IO_SeekAfterEnd");
					break;

				case SeekOrigin.Current:
					newPosition = _origin + offset + Position;
					if (newPosition < _origin)
						throw new IOException("IO.IO_SeekBeforeBegin");
					if (newPosition > _origin + _count)
						throw new IOException("IO.IO_SeekAfterEnd");
					break;

				case SeekOrigin.End:
					newPosition = _origin + _count - offset;
					if (newPosition < _origin)
						throw new IOException("IO.IO_SeekBeforeBegin");
					if (offset < 0 || newPosition > _origin + _count)
						throw new IOException("IO.IO_SeekAfterEnd");
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
			}

			_baseStream.Seek(newPosition, SeekOrigin.Begin);
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("The stream region cannot be modified.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!CanWrite)
				throw new NotSupportedException();
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);

			// TODO: we can actually exceed the bounds of the region. Add range checks.

			_baseStream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RegionStream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			_disposed = true;
			base.Dispose(disposing);
		}
	}
}
