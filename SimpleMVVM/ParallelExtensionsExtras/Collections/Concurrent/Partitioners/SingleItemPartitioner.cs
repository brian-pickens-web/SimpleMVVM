// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.Partitioners.SingleItemPartitioner
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Concurrent.Partitioners
{
  public static class SingleItemPartitioner
  {
    public static OrderablePartitioner<T> Create<T>(IEnumerable<T> source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (source is IList<T>)
        return (OrderablePartitioner<T>) new SingleItemPartitioner.SingleItemIListPartitioner<T>((IList<T>) source);
      return (OrderablePartitioner<T>) new SingleItemPartitioner.SingleItemEnumerablePartitioner<T>(source);
    }

    private sealed class SingleItemEnumerablePartitioner<T> : OrderablePartitioner<T>
    {
      private readonly IEnumerable<T> _source;

      internal SingleItemEnumerablePartitioner(IEnumerable<T> source)
        : base(true, false, true)
      {
        this._source = source;
      }

      public override bool SupportsDynamicPartitions
      {
        get
        {
          return true;
        }
      }

      public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(
        int partitionCount)
      {
        if (partitionCount < 1)
          throw new ArgumentOutOfRangeException(nameof (partitionCount));
        SingleItemPartitioner.SingleItemEnumerablePartitioner<T>.DynamicGenerator dynamicPartitioner = new SingleItemPartitioner.SingleItemEnumerablePartitioner<T>.DynamicGenerator(this._source.GetEnumerator(), false);
        return (IList<IEnumerator<KeyValuePair<long, T>>>) Enumerable.Range(0, partitionCount).Select<int, IEnumerator<KeyValuePair<long, T>>>((Func<int, IEnumerator<KeyValuePair<long, T>>>) (i => dynamicPartitioner.GetEnumerator())).ToList<IEnumerator<KeyValuePair<long, T>>>();
      }

      public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
      {
        return (IEnumerable<KeyValuePair<long, T>>) new SingleItemPartitioner.SingleItemEnumerablePartitioner<T>.DynamicGenerator(this._source.GetEnumerator(), true);
      }

      private class DynamicGenerator : IEnumerable<KeyValuePair<long, T>>, IEnumerable, IDisposable
      {
        private readonly IEnumerator<T> _sharedEnumerator;
        private long _nextAvailablePosition;
        private int _remainingPartitions;
        private bool _disposed;

        public DynamicGenerator(IEnumerator<T> sharedEnumerator, bool requiresDisposal)
        {
          this._sharedEnumerator = sharedEnumerator;
          this._nextAvailablePosition = -1L;
          this._remainingPartitions = requiresDisposal ? 1 : 0;
        }

        void IDisposable.Dispose()
        {
          if (this._disposed || Interlocked.Decrement(ref this._remainingPartitions) != 0)
            return;
          this._disposed = true;
          this._sharedEnumerator.Dispose();
        }

        public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
        {
          Interlocked.Increment(ref this._remainingPartitions);
          return this.GetEnumeratorCore();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
          return (IEnumerator) this.GetEnumerator();
        }

        private IEnumerator<KeyValuePair<long, T>> GetEnumeratorCore()
        {
          try
          {
            while (true)
            {
              T nextItem;
              long position;
              lock (this._sharedEnumerator)
              {
                if (this._sharedEnumerator.MoveNext())
                {
                  position = this._nextAvailablePosition++;
                  nextItem = this._sharedEnumerator.Current;
                }
                else
                  break;
              }
              yield return new KeyValuePair<long, T>(position, nextItem);
            }
          }
          finally
          {
            if (Interlocked.Decrement(ref this._remainingPartitions) == 0)
              this._sharedEnumerator.Dispose();
          }
        }
      }
    }

    private sealed class SingleItemIListPartitioner<T> : OrderablePartitioner<T>
    {
      private readonly IList<T> _source;

      internal SingleItemIListPartitioner(IList<T> source)
        : base(true, false, true)
      {
        this._source = source;
      }

      public override bool SupportsDynamicPartitions
      {
        get
        {
          return true;
        }
      }

      public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(
        int partitionCount)
      {
        if (partitionCount < 1)
          throw new ArgumentOutOfRangeException(nameof (partitionCount));
        IEnumerable<KeyValuePair<long, T>> dynamicPartitioner = this.GetOrderableDynamicPartitions();
        return (IList<IEnumerator<KeyValuePair<long, T>>>) Enumerable.Range(0, partitionCount).Select<int, IEnumerator<KeyValuePair<long, T>>>((Func<int, IEnumerator<KeyValuePair<long, T>>>) (i => dynamicPartitioner.GetEnumerator())).ToList<IEnumerator<KeyValuePair<long, T>>>();
      }

      public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
      {
        return SingleItemPartitioner.SingleItemIListPartitioner<T>.GetOrderableDynamicPartitionsCore(this._source, new StrongBox<int>(0));
      }

      private static IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitionsCore(
        IList<T> source,
        StrongBox<int> nextIteration)
      {
        while (true)
        {
          int iteration = Interlocked.Increment(ref nextIteration.Value) - 1;
          if (iteration >= 0 && iteration < source.Count)
            yield return new KeyValuePair<long, T>((long) iteration, source[iteration]);
          else
            break;
        }
      }
    }
  }
}
