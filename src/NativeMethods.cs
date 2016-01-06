using System;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace skwas.IO
{
	static partial class NativeMethods
	{
		[DllImport("ole32")]
		public static extern int GetHGlobalFromStream(ComTypes.IStream pstm, out IntPtr phglobal);

		[Flags]
		public enum Stgm
		{
			Direct = 0x00000000,
			Transacted = 0x00010000,
			Simple = 0x08000000,

			Read = 0x00000000,
			Write = 0x00000001,
			ReadWrite = 0x00000002,

			ShareDenyNone = 0x00000040,
			ShareDenyRead = 0x00000030,
			ShareDenyWrite = 0x00000020,
			ShareExclusive = 0x00000010,

			Priority = 0x00040000,
			DeleteOnRelease = 0x04000000,
			// #if (WINVER >= 400)
			NoScratch = 0x00100000,
			// #endif /* WINVER */

			Create = 0x00001000,
			Convert = 0x00020000,
			FailIfThere = 0x00000000,

			NoSnapShot = 0x00200000,
			// #if (_WIN32_WINNT >= 0x0500)
			Direct_SWMR = 0x00400000,
			// #endif
		}

		[Flags]
		public enum STGC : int
		{
			Default = 0,
			Overwrite = 1,
			OnlyIfCurrent = 2,
			DangerouslyCommitMerelyToDiskCache = 4,
			Consolidate = 8
		}

		public enum STATFLAG
		{	
			Default,
			NoName,
			NoOpen
		}

		[
		ComImport,
		Guid("0000000a-0000-0000-C000-000000000046"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
		]
		public interface ILockBytes
		{
			int ReadAt(
				ulong ulOffset,
				IntPtr pv,
				ulong cb,
				out ulong pcbRead
				);

			int WriteAt(
				ulong ulOffset,
				IntPtr pv,
				ulong cb,
				out ulong pcbWritten
				);

			int Flush();

			int SetSize(
				ulong cb
				);

			int LockRegion(
				ulong libOffset,
				ulong cb,
				int dwLockType
				);

			int UnlockRegion(
				ulong libOffset,
				ulong cb,
				int dwLockType
				);

			int Stat(
				out ComTypes.STATSTG pstatstg,
				int grfStatFlag
				);

		}


		[
		ComImport,
		Guid("0000000d-0000-0000-C000-000000000046"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
		]
		public interface IEnumSTATSTG
		{
			int Next(
				uint celt,
				out ComTypes.STATSTG rgelt,
				out uint pceltFetched
				);

			[PreserveSig]
			int Skip(
				uint celt
				);

			int Reset();

			int Clone(
				out IEnumSTATSTG ppenum
				);

		}

		[
		ComImport,
		Guid("0000000b-0000-0000-C000-000000000046"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
		]
		public interface IStorage
		{
			int CreateStream(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				Stgm grfMode,
				int reserved1,
				int reserved2,
			   [MarshalAs(UnmanagedType.Interface)] out ComTypes.IStream ppstm
				);

			int OpenStream(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				int reserved1,
				Stgm grfMode,
				int reserved2,
			   [MarshalAs(UnmanagedType.Interface)] out ComTypes.IStream ppstm
				);

			int CreateStorage(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				Stgm grfMode,
				int reserved1,
				int reserved2,
				[MarshalAs(UnmanagedType.Interface)] out IStorage ppstg
				);

			int OpenStorage(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				[MarshalAs(UnmanagedType.Interface)] IStorage pstgPriority,
				Stgm grfMode,
				int snbExclude,
				int reserved,
				[MarshalAs(UnmanagedType.Interface)] out IStorage ppstg
				);

			int CopyTo(
				int ciidExclude,
				IntPtr rgiidExclude,
				IntPtr snbExclude,
				[MarshalAs(UnmanagedType.Interface)] IStorage pstgDest
				);

			int MoveElementTo(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				[MarshalAs(UnmanagedType.Interface)] IStorage pstgDest,
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName,
				int grfFlags
				);

			int Commit(
				int grfCommitFlags
				);

			int Revert();

			int EnumElements(
				int reserved1,
				IntPtr reserved2,
				int reserved3,
				[MarshalAs(UnmanagedType.Interface)] out IEnumSTATSTG ppenum
				);

			int DestroyElement(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName
				);

			int RenameElement(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsOldName,
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName
				);

			int SetElementTimes(
				[MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
				ComTypes.FILETIME pctime,
				ComTypes.FILETIME patime,
				ComTypes.FILETIME pmtime
				);

			int SetClass(
				IntPtr clsid
				);

			int SetStateBits(
				int grfStateBits,
				int grfMask
				);

			int Stat(
				out ComTypes.STATSTG pstatstg,
				STATFLAG grfStatFlag
				);

		}

		public class ITS_Control_Data
		{
			public uint cdwControlData = 0;
			[MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
			public uint[] adwControlData = null;

            private ITS_Control_Data() { }
		}

		public enum ECompactionLev
		{
			COMPACT_DATA,
			COMPACT_DATA_AND_PATH
		}


		[
		ComImport,
		Guid("5d02926a-212e-11d0-9df9-00a0c922e6ec"),
		ClassInterface(ClassInterfaceType.AutoDual)
		]
		public class TStorage
		{
		}

		[
		ComImport,
		Guid("88CC31DE-27AB-11D0-9DF9-00A0C922E6EC"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
		]
		public interface ITStorage
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			IStorage StgCreateDocfile(
				[In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
				Stgm grfMode,
				int reserved
				);

			[return: MarshalAs(UnmanagedType.Interface)]
			IStorage StgCreateDocfileOnILockBytes(
				ILockBytes plkbyt,
				Stgm grfMode,
				int reserved
				);

			int StgIsStorageFile(
				[In, MarshalAs(UnmanagedType.BStr)] string pwcsName
				);

			int StgIsStorageILockBytes(
				ILockBytes plkbyt
				);

			[return: MarshalAs(UnmanagedType.Interface)]
			IStorage StgOpenStorage(
				[In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
				IStorage pstgPriority,
				[In, MarshalAs(UnmanagedType.I4)] Stgm grfMode,
				IntPtr snbExclude,
				[In, MarshalAs(UnmanagedType.U4)] int reserved
				);

			[return: MarshalAs(UnmanagedType.Interface)]
			IStorage StgOpenStorageOnILockBytes(
				ILockBytes plkbyt,
				IStorage pStgPriority,
				Stgm grfMode,
				IntPtr snbExclude,
				int reserved
				);

			int StgSetTimes(
				[In, MarshalAs(UnmanagedType.BStr)] string lpszName,
				ComTypes.FILETIME pctime,
				ComTypes.FILETIME patime,
				ComTypes.FILETIME pmtime
				);

			int SetControlData(
				ITS_Control_Data pControlData
				);

			int DefaultControlData(
				ITS_Control_Data ppControlData
				);

			int Compact(
				[In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
				ECompactionLev iLev
				);
		}
	}
}
