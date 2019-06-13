// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ObservableConcurrentDictionary`2
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent
{
  [DebuggerDisplay("Count={Count}")]
  public class ObservableConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    private readonly SynchronizationContext _context;
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

    public ObservableConcurrentDictionary()
    {
      this._context = AsyncOperationManager.SynchronizationContext;
      this._dictionary = new ConcurrentDictionary<TKey, TValue>();
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
        propertyHandler((object) this, new PropertyChangedEventArgs("Keys"));
        propertyHandler((object) this, new PropertyChangedEventArgs("Values"));
      }), (object) null);
    }

    private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
    {
      return this.TryAddWithNotification(item.Key, item.Value);
    }

    private bool TryAddWithNotification(TKey key, TValue value)
    {
      bool flag = this._dictionary.TryAdd(key, value);
      if (flag)
        this.NotifyObserversOfChange();
      return flag;
    }

    private bool TryRemoveWithNotification(TKey key, out TValue value)
    {
      bool flag = this._dictionary.TryRemove(key, out value);
      if (flag)
        this.NotifyObserversOfChange();
      return flag;
    }

    private void UpdateWithNotification(TKey key, TValue value)
    {
      this._dictionary[key] = value;
      this.NotifyObserversOfChange();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(
      KeyValuePair<TKey, TValue> item)
    {
      this.TryAddWithNotification(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
      this._dictionary.Clear();
      this.NotifyObserversOfChange();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
      KeyValuePair<TKey, TValue> item)
    {
      return ((ICollection<KeyValuePair<TKey, TValue>>) this._dictionary).Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
      KeyValuePair<TKey, TValue>[] array,
      int arrayIndex)
    {
      ((ICollection<KeyValuePair<TKey, TValue>>) this._dictionary).CopyTo(array, arrayIndex);
    }

    int ICollection<KeyValuePair<TKey, TValue>>.Count
    {
      get
      {
        return this._dictionary.Count;
      }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
      get
      {
        return ((ICollection<KeyValuePair<TKey, TValue>>) this._dictionary).IsReadOnly;
      }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(
      KeyValuePair<TKey, TValue> item)
    {
      TValue obj;
      return this.TryRemoveWithNotification(item.Key, out obj);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return this._dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this._dictionary.GetEnumerator();
    }

    public void Add(TKey key, TValue value)
    {
      this.TryAddWithNotification(key, value);
    }

    public bool ContainsKey(TKey key)
    {
      return this._dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys
    {
      get
      {
        return this._dictionary.Keys;
      }
    }

    public bool Remove(TKey key)
    {
      TValue obj;
      return this.TryRemoveWithNotification(key, out obj);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return this._dictionary.TryGetValue(key, out value);
    }

    public ICollection<TValue> Values
    {
      get
      {
        return this._dictionary.Values;
      }
    }

    public TValue this[TKey key]
    {
      get
      {
        return this._dictionary[key];
      }
      set
      {
        this.UpdateWithNotification(key, value);
      }
    }
  }
}
