// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ConcurrentPriorityQueue`2
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
  [DebuggerDisplay("Count={Count}")]
  public class ConcurrentPriorityQueue<TKey, TValue> : IProducerConsumerCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICollection, IEnumerable
    where TKey : IComparable<TKey>
  {
    private readonly object _syncLock = new object();
    private readonly ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap _minHeap = new ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap();

    public ConcurrentPriorityQueue()
    {
    }

    public ConcurrentPriorityQueue(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
      if (collection == null)
        throw new ArgumentNullException(nameof (collection));
      foreach (KeyValuePair<TKey, TValue> entry in collection)
        this._minHeap.Insert(entry);
    }

    public void Enqueue(TKey priority, TValue value)
    {
      this.Enqueue(new KeyValuePair<TKey, TValue>(priority, value));
    }

    public void Enqueue(KeyValuePair<TKey, TValue> item)
    {
      lock (this._syncLock)
        this._minHeap.Insert(item);
    }

    public bool TryDequeue(out KeyValuePair<TKey, TValue> result)
    {
      result = new KeyValuePair<TKey, TValue>();
      lock (this._syncLock)
      {
        if (this._minHeap.Count > 0)
        {
          result = this._minHeap.Remove();
          return true;
        }
      }
      return false;
    }

    public bool TryPeek(out KeyValuePair<TKey, TValue> result)
    {
      result = new KeyValuePair<TKey, TValue>();
      lock (this._syncLock)
      {
        if (this._minHeap.Count > 0)
        {
          result = this._minHeap.Peek();
          return true;
        }
      }
      return false;
    }

    public void Clear()
    {
      lock (this._syncLock)
        this._minHeap.Clear();
    }

    public bool IsEmpty
    {
      get
      {
        return this.Count == 0;
      }
    }

    public int Count
    {
      get
      {
        lock (this._syncLock)
          return this._minHeap.Count;
      }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
      lock (this._syncLock)
        this._minHeap.Items.CopyTo(array, index);
    }

    public KeyValuePair<TKey, TValue>[] ToArray()
    {
      lock (this._syncLock)
      {
        ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap minBinaryHeap = new ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap(this._minHeap);
        KeyValuePair<TKey, TValue>[] keyValuePairArray = new KeyValuePair<TKey, TValue>[this._minHeap.Count];
        for (int index = 0; index < keyValuePairArray.Length; ++index)
          keyValuePairArray[index] = minBinaryHeap.Remove();
        return keyValuePairArray;
      }
    }

    bool IProducerConsumerCollection<KeyValuePair<TKey, TValue>>.TryAdd(
      KeyValuePair<TKey, TValue> item)
    {
      this.Enqueue(item);
      return true;
    }

    bool IProducerConsumerCollection<KeyValuePair<TKey, TValue>>.TryTake(
      out KeyValuePair<TKey, TValue> item)
    {
      return this.TryDequeue(out item);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<TKey, TValue>>) this.ToArray()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    void ICollection.CopyTo(Array array, int index)
    {
      lock (this._syncLock)
        ((ICollection) this._minHeap.Items).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return true;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return this._syncLock;
      }
    }

    private sealed class MinBinaryHeap
    {
      private readonly List<KeyValuePair<TKey, TValue>> _items;

      public MinBinaryHeap()
      {
        this._items = new List<KeyValuePair<TKey, TValue>>();
      }

      public MinBinaryHeap(
        ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap heapToCopy)
      {
        this._items = new List<KeyValuePair<TKey, TValue>>((IEnumerable<KeyValuePair<TKey, TValue>>) heapToCopy.Items);
      }

      public void Clear()
      {
        this._items.Clear();
      }

      public void Insert(TKey key, TValue value)
      {
        this.Insert(new KeyValuePair<TKey, TValue>(key, value));
      }

      public void Insert(KeyValuePair<TKey, TValue> entry)
      {
        this._items.Add(entry);
        int index1 = this._items.Count - 1;
        if (index1 == 0)
          return;
        int index2;
        for (; index1 > 0; index1 = index2)
        {
          index2 = (index1 - 1) / 2;
          KeyValuePair<TKey, TValue> keyValuePair = this._items[index2];
          if (entry.Key.CompareTo(keyValuePair.Key) < 0)
            this._items[index1] = keyValuePair;
          else
            break;
        }
        this._items[index1] = entry;
      }

      public KeyValuePair<TKey, TValue> Peek()
      {
        if (this._items.Count == 0)
          throw new InvalidOperationException("The heap is empty.");
        return this._items[0];
      }

      public KeyValuePair<TKey, TValue> Remove()
      {
        if (this._items.Count == 0)
          throw new InvalidOperationException("The heap is empty.");
        KeyValuePair<TKey, TValue> keyValuePair1 = this._items[0];
        if (this._items.Count <= 2)
        {
          this._items.RemoveAt(0);
        }
        else
        {
          this._items[0] = this._items[this._items.Count - 1];
          this._items.RemoveAt(this._items.Count - 1);
          int index1 = 0;
          int index2 = 0;
          while (true)
          {
            int index3 = 2 * index1 + 1;
            int index4 = index3 + 1;
            if (index3 < this._items.Count)
            {
              KeyValuePair<TKey, TValue> keyValuePair2 = this._items[index1];
              if (this._items[index3].Key.CompareTo(keyValuePair2.Key) < 0)
                index2 = index3;
              if (index4 < this._items.Count)
              {
                KeyValuePair<TKey, TValue> keyValuePair3 = this._items[index2];
                if (this._items[index4].Key.CompareTo(keyValuePair3.Key) < 0)
                  index2 = index4;
              }
              if (index1 != index2)
              {
                KeyValuePair<TKey, TValue> keyValuePair3 = this._items[index1];
                this._items[index1] = this._items[index2];
                this._items[index2] = keyValuePair3;
                index1 = index2;
              }
              else
                break;
            }
            else
              break;
          }
        }
        return keyValuePair1;
      }

      public int Count
      {
        get
        {
          return this._items.Count;
        }
      }

      internal List<KeyValuePair<TKey, TValue>> Items
      {
        get
        {
          return this._items;
        }
      }
    }
  }
}
