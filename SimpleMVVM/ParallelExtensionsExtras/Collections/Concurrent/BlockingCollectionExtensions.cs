// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.BlockingCollectionExtensions
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Collections.Concurrent
{
  public static class BlockingCollectionExtensions
  {
    public static Partitioner<T> GetConsumingPartitioner<T>(
      this BlockingCollection<T> collection)
    {
      return (Partitioner<T>) new BlockingCollectionExtensions.BlockingCollectionPartitioner<T>(collection);
    }

    public static void AddFromEnumerable<T>(
      this BlockingCollection<T> target,
      IEnumerable<T> source,
      bool completeAddingWhenDone)
    {
      try
      {
        foreach (T obj in source)
          target.Add(obj);
      }
      finally
      {
        if (completeAddingWhenDone)
          target.CompleteAdding();
      }
    }

    public static IDisposable AddFromObservable<T>(
      this BlockingCollection<T> target,
      IObservable<T> source,
      bool completeAddingWhenDone)
    {
      if (target == null)
        throw new ArgumentNullException(nameof (target));
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      return source.Subscribe((IObserver<T>) new DelegateBasedObserver<T>((Action<T>) (item => target.Add(item)), (Action<Exception>) (error =>
      {
        if (!completeAddingWhenDone)
          return;
        target.CompleteAdding();
      }), (Action) (() =>
      {
        if (!completeAddingWhenDone)
          return;
        target.CompleteAdding();
      })));
    }

    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
      this BlockingCollection<T> collection)
    {
      return collection.ToProducerConsumerCollection<T>(-1);
    }

    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
      this BlockingCollection<T> collection,
      int millisecondsTimeout)
    {
      return (IProducerConsumerCollection<T>) new BlockingCollectionExtensions.ProducerConsumerWrapper<T>(collection, millisecondsTimeout, new CancellationToken());
    }

    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
      this BlockingCollection<T> collection,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      return (IProducerConsumerCollection<T>) new BlockingCollectionExtensions.ProducerConsumerWrapper<T>(collection, millisecondsTimeout, cancellationToken);
    }

    private class BlockingCollectionPartitioner<T> : Partitioner<T>
    {
      private BlockingCollection<T> _collection;

      internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
      {
        if (collection == null)
          throw new ArgumentNullException(nameof (collection));
        this._collection = collection;
      }

      public override bool SupportsDynamicPartitions
      {
        get
        {
          return true;
        }
      }

      public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
      {
        if (partitionCount < 1)
          throw new ArgumentOutOfRangeException(nameof (partitionCount));
        IEnumerable<T> dynamicPartitioner = this.GetDynamicPartitions();
        return (IList<IEnumerator<T>>) Enumerable.Range(0, partitionCount).Select<int, IEnumerator<T>>((Func<int, IEnumerator<T>>) (_ => dynamicPartitioner.GetEnumerator())).ToArray<IEnumerator<T>>();
      }

      public override IEnumerable<T> GetDynamicPartitions()
      {
        return this._collection.GetConsumingEnumerable();
      }
    }

    internal sealed class ProducerConsumerWrapper<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
      private readonly BlockingCollection<T> _collection;
      private readonly int _millisecondsTimeout;
      private readonly CancellationToken _cancellationToken;

      public ProducerConsumerWrapper(
        BlockingCollection<T> collection,
        int millisecondsTimeout,
        CancellationToken cancellationToken)
      {
        if (collection == null)
          throw new ArgumentNullException("bc");
        if (millisecondsTimeout < -1)
          throw new ArgumentOutOfRangeException(nameof (millisecondsTimeout));
        this._collection = collection;
        this._millisecondsTimeout = millisecondsTimeout;
        this._cancellationToken = cancellationToken;
      }

      public void CopyTo(T[] array, int index)
      {
        this._collection.CopyTo(array, index);
      }

      public T[] ToArray()
      {
        return this._collection.ToArray();
      }

      public bool TryAdd(T item)
      {
        return this._collection.TryAdd(item, this._millisecondsTimeout, this._cancellationToken);
      }

      public bool TryTake(out T item)
      {
        return this._collection.TryTake(out item, this._millisecondsTimeout, this._cancellationToken);
      }

      public IEnumerator<T> GetEnumerator()
      {
        return ((IEnumerable<T>) this._collection).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) this.GetEnumerator();
      }

      public void CopyTo(Array array, int index)
      {
        ((ICollection) this._collection).CopyTo(array, index);
      }

      public int Count
      {
        get
        {
          return this._collection.Count;
        }
      }

      public bool IsSynchronized
      {
        get
        {
          return ((ICollection) this._collection).IsSynchronized;
        }
      }

      public object SyncRoot
      {
        get
        {
          return ((ICollection) this._collection).SyncRoot;
        }
      }
    }
  }
}
