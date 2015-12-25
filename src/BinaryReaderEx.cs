using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace skwas.IO
{
	/// <summary>
	/// Inherited from <see cref="BinaryReader"/>, extended with a functionality to read structures/objects and strings.
	/// </summary>
	public class BinaryReaderEx : BinaryReader
	{
		private static readonly Type ColorType = typeof(Color);

		/// <summary>
		/// Initializes a new instance the <see cref="BinaryReaderEx"/> based on the supplied stream and using <see cref="UTF8Encoding"/>.
		/// </summary>
		/// <param name="input">The supplied stream.</param>
		public BinaryReaderEx(Stream input)
			: base(input)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryReaderEx"/> based on the supplied stream and a specific character encoding.
		/// </summary>
		/// <param name="input">The supplied stream.</param>
		/// <param name="encoding">The character encoding.</param>
		public BinaryReaderEx(Stream input, Encoding encoding)
			: base(input, encoding)
		{
		}

		~BinaryReaderEx()
		{
			Dispose(false);
		}

		/// <summary>
		/// Reads a structure/class of <typeparamref name="T"/> from the current stream and advances the current position of the stream by the size of the structure in bytes.
		/// </summary>
		/// <typeparam name="T">The type to read.</typeparam>
		/// <returns>An object of specified type, read from the stream.</returns>
		public T ReadStruct<T>()
		{
			return (T)ReadStruct(typeof (T));
		}

		/// <summary>
		/// Reads a structure/class of <paramref name="type"/> from the current stream and advances the current position of the stream by the size of the structure in bytes.
		/// </summary>
		/// <param name="type">The type to read.</param>
		/// <returns>An object of specified type, read from the stream.</returns>
		public object ReadStruct(Type type)
		{
			// Take care of nullable types. Although we can't read null values from a stream, the caller expects the underlying type.
			var targetType = Nullable.GetUnderlyingType(type) ?? type;

			// Bools are deserialized as byte 0 = false, otherwise = true.
			if (targetType == typeof(bool))
				return ReadByte() > 0;

			if (targetType == ColorType)
				// The color struct cannot be deserialized using interop.
				return Color.FromArgb(ReadByte(), ReadByte(), ReadByte(), ReadByte());

			// If the type is enum, we have to deserialize using underlying type.
			Type enumType = null;
			if (targetType.IsEnum)
			{
				enumType = targetType;
				targetType = Enum.GetUnderlyingType(targetType);
			}

			// Determine size of requested structure and read the struct.
			var structSize = Marshal.SizeOf(targetType);
			var value = ReadStruct(targetType, structSize);

			// If enum, cast back to the enum type.
			if (enumType != null)
				value = Enum.ToObject(enumType, value);

			return value;
		}

		/// <summary>
		/// Reads a structure/class of <typeparamref name="T"/> from the current stream and advances the current position of the stream by the size of the structure in bytes.
		/// </summary>
		/// <typeparam name="T">The type to read.</typeparam>
		/// <param name="structSize">The size of the structure.</param>
		/// <returns>An object of specified type, read from the stream.</returns>
		public object ReadStruct<T>(int structSize)
		{
			return ReadStruct(typeof(T), structSize);
		}

		/// <summary>
		/// Reads a structure/class of specified <paramref name="type"/> from the current stream and advances the current position of the stream by the size of the structure in bytes.
		/// </summary>
		/// <param name="type">The type to read.</param>
		/// <param name="structSize">The size of the structure.</param>
		/// <returns>An object of specified type, read from the stream.</returns>
		/// <exception cref="EndOfStreamException">Thrown when the requested <paramref name="structSize"/> exceeds the remaining available data.</exception>
		public object ReadStruct(Type type, int structSize)
		{
			// Read bytes from stream.
			var structData = ReadBytes(structSize);

			// Check if data read matches requested.
			if (structData.Length != structSize)
				throw new EndOfStreamException("Unable to read beyond the end of the stream.");

			// Copy byte array to unmanaged memory block.
			object value;
			var hBuffer = IntPtr.Zero;
			try
			{
				hBuffer = Marshal.AllocHGlobal(structSize);
				Marshal.Copy(structData, 0, hBuffer, structSize);
				// Marshal the unmanaged memory pointer back to a typed object.
				value = Marshal.PtrToStructure(hBuffer, type);
			}
			finally
			{
				// Free memory.
				if (hBuffer != IntPtr.Zero) Marshal.FreeHGlobal(hBuffer);
			}

			return value;
		}

		/// <summary>
		/// Reads a string from the current stream, using specified number of characters.
		/// </summary>
		/// <param name="characters">The number of characters to read.</param>
		/// <returns>The string.</returns>
		public string ReadString(int characters)
		{
			var buffer = new StringBuilder();
			for (var i = 0; i < characters; i++)
			{
				var c = ReadChar();
				buffer.Append(c);
			}
			return buffer.ToString();
		}

		/// <summary>
		/// Reads a string from the current stream. The stream is read until the specified characters are found in order. The terminating characters are not included in the returned string.
		/// </summary>
		/// <param name="terminatingCharacters">The terminating characters.</param>
		/// <returns>The string.</returns>
		public string ReadString(char[] terminatingCharacters)
		{
			var tc = (IList<char>)(terminatingCharacters);
			var buffer = new StringBuilder();
			while (true)
			{
				var c = ReadChar();
				if (tc.Contains(c)) break;
				buffer.Append(c);
			}
			return buffer.ToString();
		}

		/// <summary>
		/// Reads a string from the current stream. The stream is read until the specified character is found. The terminating character is not included in the returned string.
		/// </summary>
		/// <param name="terminatingCharacter">The terminating character.</param>
		/// <returns>The string.</returns>
		public string ReadString(char terminatingCharacter)
		{
			return ReadString(new[] {terminatingCharacter});
		}
	}	
}
