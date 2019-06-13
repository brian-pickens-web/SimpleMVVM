// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.ProducerConsumerCollectionExtensions
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;

namespace System.Collections.Concurrent
{
  public static class ProducerConsumerCollectionExtensions
  {
    public static void Clear<T>(this IProducerConsumerCollection<T> collection)
    {
      T obj;
    #pragma warning disable CS0642
        do
            // ReSharper disable once EmptyEmbeddedStatement
            ;
        while (collection.TryTake(out obj));
    #pragma warning restore CS0642
    }

    public static IEnumerable<T> GetConsumingEnumerable<T>(
      this IProducerConsumerCollection<T> collection)
    {
      T item;
      while (collection.TryTake(out item))
        yield return item;
    }

    public static void AddFromEnumerable<T>(
      this IProducerConsumerCollection<T> target,
      IEnumerable<T> source)
    {
      foreach (T obj in source)
        target.TryAdd(obj);
    }

    public static IDisposable AddFromObservable<T>(
      this IProducerConsumerCollection<T> target,
      IObservable<T> source)
    {
      if (target == null)
        throw new ArgumentNullException(nameof (target));
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      return source.Subscribe((IObserver<T>) new DelegateBasedObserver<T>((Action<T>) (item => target.TryAdd(item)), (Action<Exception>) (error => {}), (Action) (() => {})));
    }

    public static IProducerConsumerCollection<T> ToProducerOnlyCollection<T>(
      this IProducerConsumerCollection<T> collection)
    {
      return (IProducerConsumerCollection<T>) new ProducerConsumerCollectionExtensions.ProduceOrConsumeOnlyCollection<T>(collection, true);
    }

    public static IProducerConsumerCollection<T> ToConsumerOnlyCollection<T>(
      this IProducerConsumerCollection<T> collection)
    {
      return (IProducerConsumerCollection<T>) new ProducerConsumerCollectionExtensions.ProduceOrConsumeOnlyCollection<T>(collection, false);
    }

    private sealed class ProduceOrConsumeOnlyCollection<T> : ProducerConsumerCollectionBase<T>
    {
      private readonly bool _produceOnly;

      public ProduceOrConsumeOnlyCollection(
        IProducerConsumerCollection<T> contained,
        bool produceOnly)
        : base(contained)
      {
        this._produceOnly = produceOnly;
      }

      protected override bool TryAdd(T item)
      {
        if (!this._produceOnly)
          throw new NotSupportedException();
        return base.TryAdd(item);
      }

      protected override bool TryTake(out T item)
      {
        if (this._produceOnly)
          throw new NotSupportedException();
        return base.TryTake(out item);
      }
    }
  }
}
