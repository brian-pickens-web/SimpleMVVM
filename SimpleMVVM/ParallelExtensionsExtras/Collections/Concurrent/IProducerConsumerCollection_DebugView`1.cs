// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.IProducerConsumerCollection_DebugView`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Diagnostics;

namespace System.Collections.Concurrent
{
  internal sealed class IProducerConsumerCollection_DebugView<T>
  {
    private IProducerConsumerCollection<T> _collection;

    public IProducerConsumerCollection_DebugView(IProducerConsumerCollection<T> collection)
    {
      this._collection = collection;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Values
    {
      get
      {
        return this._collection.ToArray();
      }
    }
  }
}
