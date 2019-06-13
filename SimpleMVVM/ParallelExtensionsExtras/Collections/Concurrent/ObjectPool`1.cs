// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ObjectPool`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
  [DebuggerDisplay("Count={Count}")]
  [DebuggerTypeProxy(typeof (IProducerConsumerCollection_DebugView<>))]
  public sealed class ObjectPool<T> : ProducerConsumerCollectionBase<T>
  {
    private readonly Func<T> _generator;

    public ObjectPool(Func<T> generator)
      : this(generator, (IProducerConsumerCollection<T>) new ConcurrentQueue<T>())
    {
    }

    public ObjectPool(Func<T> generator, IProducerConsumerCollection<T> collection)
      : base(collection)
    {
      if (generator == null)
        throw new ArgumentNullException(nameof (generator));
      this._generator = generator;
    }

    public void PutObject(T item)
    {
      base.TryAdd(item);
    }

    public T GetObject()
    {
      T obj;
      if (!base.TryTake(out obj))
        return this._generator();
      return obj;
    }

    public T[] ToArrayAndClear()
    {
      List<T> objList = new List<T>();
      T obj;
      while (base.TryTake(out obj))
        objList.Add(obj);
      return objList.ToArray();
    }

    protected override bool TryAdd(T item)
    {
      this.PutObject(item);
      return true;
    }

    protected override bool TryTake(out T item)
    {
      item = this.GetObject();
      return true;
    }
  }
}
