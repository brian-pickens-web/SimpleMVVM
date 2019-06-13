// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ProducerConsumerCollectionBase`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;

namespace System.Collections.Concurrent
{
  [Serializable]
  public abstract class ProducerConsumerCollectionBase<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
  {
    private readonly IProducerConsumerCollection<T> _contained;

    protected ProducerConsumerCollectionBase(IProducerConsumerCollection<T> contained)
    {
      if (contained == null)
        throw new ArgumentNullException(nameof (contained));
      this._contained = contained;
    }

    protected IProducerConsumerCollection<T> ContainedCollection
    {
      get
      {
        return this._contained;
      }
    }

    protected virtual bool TryAdd(T item)
    {
      return this._contained.TryAdd(item);
    }

    protected virtual bool TryTake(out T item)
    {
      return this._contained.TryTake(out item);
    }

    bool IProducerConsumerCollection<T>.TryAdd(T item)
    {
      return this.TryAdd(item);
    }

    bool IProducerConsumerCollection<T>.TryTake(out T item)
    {
      return this.TryTake(out item);
    }

    public int Count
    {
      get
      {
        return this._contained.Count;
      }
    }

    public T[] ToArray()
    {
      return this._contained.ToArray();
    }

    public void CopyTo(T[] array, int index)
    {
      this._contained.CopyTo(array, index);
    }

    void ICollection.CopyTo(Array array, int index)
    {
      this._contained.CopyTo(array, index);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return this._contained.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return this._contained.IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return this._contained.SyncRoot;
      }
    }
  }
}
