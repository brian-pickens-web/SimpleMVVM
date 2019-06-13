// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.Partitioners.ChunkPartitioner`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent.Partitioners
{
  internal sealed class ChunkPartitioner<T> : OrderablePartitioner<T>
  {
    private readonly IEnumerable<T> _source;
    private readonly Func<int, int> _nextChunkSizeFunc;

    public ChunkPartitioner(IEnumerable<T> source, Func<int, int> nextChunkSizeFunc)
      : base(true, true, true)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (nextChunkSizeFunc == null)
        throw new ArgumentNullException(nameof (nextChunkSizeFunc));
      this._source = source;
      this._nextChunkSizeFunc = nextChunkSizeFunc;
    }

    public ChunkPartitioner(IEnumerable<T> source, int chunkSize)
      : this(source, (Func<int, int>) (prev => chunkSize))
    {
      if (chunkSize <= 0)
        throw new ArgumentOutOfRangeException(nameof (chunkSize));
    }

    public ChunkPartitioner(IEnumerable<T> source, int minChunkSize, int maxChunkSize)
      : this(source, ChunkPartitioner<T>.CreateFuncFromMinAndMax(minChunkSize, maxChunkSize))
    {
      if (minChunkSize <= 0 || minChunkSize > maxChunkSize)
        throw new ArgumentOutOfRangeException(nameof (minChunkSize));
    }

    private static Func<int, int> CreateFuncFromMinAndMax(int minChunkSize, int maxChunkSize)
    {
      return (Func<int, int>) (prev =>
      {
        if (prev < minChunkSize)
          return minChunkSize;
        if (prev >= maxChunkSize)
          return maxChunkSize;
        int num = prev * 2;
        if (num >= maxChunkSize || num < 0)
          return maxChunkSize;
        return num;
      });
    }

    public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(
      int partitionCount)
    {
      if (partitionCount <= 0)
        throw new ArgumentOutOfRangeException(nameof (partitionCount));
      IEnumerator<KeyValuePair<long, T>>[] enumeratorArray = new IEnumerator<KeyValuePair<long, T>>[partitionCount];
      IEnumerable<KeyValuePair<long, T>> dynamicPartitions = this.GetOrderableDynamicPartitions(true);
      for (int index = 0; index < partitionCount; ++index)
        enumeratorArray[index] = dynamicPartitions.GetEnumerator();
      return (IList<IEnumerator<KeyValuePair<long, T>>>) enumeratorArray;
    }

    public override bool SupportsDynamicPartitions
    {
      get
      {
        return true;
      }
    }

    public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
    {
      return (IEnumerable<KeyValuePair<long, T>>) new ChunkPartitioner<T>.EnumerableOfEnumerators(this, false);
    }

    private IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions(
      bool referenceCountForDisposal)
    {
      return (IEnumerable<KeyValuePair<long, T>>) new ChunkPartitioner<T>.EnumerableOfEnumerators(this, referenceCountForDisposal);
    }

    private class EnumerableOfEnumerators : IEnumerable<KeyValuePair<long, T>>, IEnumerable, IDisposable
    {
      private readonly object _sharedLock = new object();
      private readonly ChunkPartitioner<T> _parentPartitioner;
      private readonly IEnumerator<T> _sharedEnumerator;
      private long _nextSharedIndex;
      private int _activeEnumerators;
      private bool _noMoreElements;
      private bool _disposed;
      private bool _referenceCountForDisposal;

      public EnumerableOfEnumerators(
        ChunkPartitioner<T> parentPartitioner,
        bool referenceCountForDisposal)
      {
        if (parentPartitioner == null)
          throw new ArgumentNullException(nameof (parentPartitioner));
        this._parentPartitioner = parentPartitioner;
        this._sharedEnumerator = parentPartitioner._source.GetEnumerator();
        this._nextSharedIndex = -1L;
        this._referenceCountForDisposal = referenceCountForDisposal;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) this.GetEnumerator();
      }

      public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
      {
        if (this._referenceCountForDisposal)
          Interlocked.Increment(ref this._activeEnumerators);
        return (IEnumerator<KeyValuePair<long, T>>) new ChunkPartitioner<T>.EnumerableOfEnumerators.Enumerator(this);
      }

      private void DisposeEnumerator(
        ChunkPartitioner<T>.EnumerableOfEnumerators.Enumerator enumerator)
      {
        if (!this._referenceCountForDisposal || Interlocked.Decrement(ref this._activeEnumerators) != 0)
          return;
        this._sharedEnumerator.Dispose();
      }

      public void Dispose()
      {
        if (this._disposed)
          return;
        if (!this._referenceCountForDisposal)
          this._sharedEnumerator.Dispose();
        this._disposed = true;
      }

      private class Enumerator : IEnumerator<KeyValuePair<long, T>>, IDisposable, IEnumerator
      {
        private List<KeyValuePair<long, T>> _currentChunk = new List<KeyValuePair<long, T>>();
        private ChunkPartitioner<T>.EnumerableOfEnumerators _parentEnumerable;
        private int _currentChunkCurrentIndex;
        private int _lastRequestedChunkSize;
        private bool _disposed;

        public Enumerator(
          ChunkPartitioner<T>.EnumerableOfEnumerators parentEnumerable)
        {
          if (parentEnumerable == null)
            throw new ArgumentNullException(nameof (parentEnumerable));
          this._parentEnumerable = parentEnumerable;
        }

        public bool MoveNext()
        {
          if (this._disposed)
            throw new ObjectDisposedException(this.GetType().Name);
          ++this._currentChunkCurrentIndex;
          if (this._currentChunkCurrentIndex >= 0 && this._currentChunkCurrentIndex < this._currentChunk.Count)
            return true;
          int num = this._parentEnumerable._parentPartitioner._nextChunkSizeFunc(this._lastRequestedChunkSize);
          if (num <= 0)
            throw new InvalidOperationException("Invalid chunk size requested: chunk sizes must be positive.");
          this._lastRequestedChunkSize = num;
          this._currentChunk.Clear();
          this._currentChunkCurrentIndex = 0;
          if (num > this._currentChunk.Capacity)
            this._currentChunk.Capacity = num;
          lock (this._parentEnumerable._sharedEnumerator)
          {
            if (this._parentEnumerable._noMoreElements)
              return false;
            for (int index = 0; index < num; ++index)
            {
              if (!this._parentEnumerable._sharedEnumerator.MoveNext())
              {
                this._parentEnumerable._noMoreElements = true;
                return this._currentChunk.Count > 0;
              }
              ++this._parentEnumerable._nextSharedIndex;
              this._currentChunk.Add(new KeyValuePair<long, T>(this._parentEnumerable._nextSharedIndex, this._parentEnumerable._sharedEnumerator.Current));
            }
          }
          return true;
        }

        public KeyValuePair<long, T> Current
        {
          get
          {
            if (this._currentChunkCurrentIndex >= this._currentChunk.Count)
              throw new InvalidOperationException("There is no current item.");
            return this._currentChunk[this._currentChunkCurrentIndex];
          }
        }

        public void Dispose()
        {
          if (this._disposed)
            return;
          this._parentEnumerable.DisposeEnumerator(this);
          this._disposed = true;
        }

        object IEnumerator.Current
        {
          get
          {
            return (object) this.Current;
          }
        }

        public void Reset()
        {
          throw new NotSupportedException();
        }
      }
    }
  }
}
