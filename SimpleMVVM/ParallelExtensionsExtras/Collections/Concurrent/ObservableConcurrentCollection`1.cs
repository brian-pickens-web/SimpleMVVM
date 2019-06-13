// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ObservableConcurrentCollection`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent
{
  [DebuggerTypeProxy(typeof (IProducerConsumerCollection_DebugView<>))]
  [DebuggerDisplay("Count={Count}")]
  public class ObservableConcurrentCollection<T> : ProducerConsumerCollectionBase<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    private readonly SynchronizationContext _context;

    public ObservableConcurrentCollection()
      : this((IProducerConsumerCollection<T>) new ConcurrentQueue<T>())
    {
    }

    public ObservableConcurrentCollection(IProducerConsumerCollection<T> collection)
      : base(collection)
    {
      this._context = AsyncOperationManager.SynchronizationContext;
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyObserversOfChange()
    {
      NotifyCollectionChangedEventHandler collectionHandler = this.CollectionChanged;
      PropertyChangedEventHandler propertyHandler = this.PropertyChanged;
      if (collectionHandler == null && propertyHandler == null)
        return;
      this._context.Post((SendOrPostCallback) (s =>
      {
        if (collectionHandler != null)
          collectionHandler((object) this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        if (propertyHandler == null)
          return;
        propertyHandler((object) this, new PropertyChangedEventArgs("Count"));
      }), (object) null);
    }

    protected override bool TryAdd(T item)
    {
      bool flag = base.TryAdd(item);
      if (flag)
        this.NotifyObserversOfChange();
      return flag;
    }

    protected override bool TryTake(out T item)
    {
      bool flag = base.TryTake(out item);
      if (flag)
        this.NotifyObserversOfChange();
      return flag;
    }
  }
}
