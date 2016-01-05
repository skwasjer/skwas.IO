using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace skwas.IO
{
	public abstract class ChunkFile<TChunk> 
		: IChunkFile
		where TChunk : class, IChunk
	{
		private bool _disposed;
		private ObservableCollection<TChunk> _chunks;

		protected ChunkFile()
		{
		}

		~ChunkFile()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				// Dispose managed.
				if (_chunks != null)
				{
					_chunks.CollectionChanged -= _chunks_CollectionChanged;

					foreach (var chunk in Chunks.Where(c => c is IDisposable))
						((IDisposable)chunk).Dispose();
				}
			}
			// Dispose unmanaged.

			_disposed = true;
		}

		/// <summary>
		/// Releases all resources associated with this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected bool IsDisposed
		{
			get { return _disposed; }
		}

		IList IChunkFile.Chunks
		{
			get { return Chunks; }
		}

		/// <summary>
		/// Gets a collection of all chunks in the ChunkFile.
		/// </summary>
		public virtual Collection<TChunk> Chunks
		{
			get
			{
				if (_disposed) throw new ObjectDisposedException(GetType().Name);
				if (_chunks == null)
				{
					_chunks = new ObservableCollection<TChunk>();
					_chunks.CollectionChanged += _chunks_CollectionChanged;
				}
				return _chunks;
			}
		}

		private void _chunks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (IChunk c in e.OldItems) 
						c.ParentFile = null;
					break;
				case NotifyCollectionChangedAction.Add:
					foreach (IChunk c in e.NewItems)
						c.ParentFile = this;
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (IChunk c in e.OldItems)
						c.ParentFile = null;
					foreach (IChunk c in e.NewItems)
						c.ParentFile = this;
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		public virtual void Load(string filename)
		{
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				Load(fs);
		}

		public abstract void Load(Stream stream);

		public virtual void Save(string filename)
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);

			using (var fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				Save(fs);
		}

		public abstract void Save(Stream stream);
	}
}
