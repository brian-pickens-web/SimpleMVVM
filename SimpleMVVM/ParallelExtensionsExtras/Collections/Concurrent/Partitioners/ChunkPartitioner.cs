// Decompiled with JetBrains decompiler
// Type: System.Collections.Concurrent.Partitioners.ChunkPartitioner
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

using System.Collections.Generic;

namespace System.Collections.Concurrent.Partitioners
{
  public static class ChunkPartitioner
  {
    public static OrderablePartitioner<TSource> Create<TSource>(
      IEnumerable<TSource> source,
      Func<int, int> nextChunkSizeFunc)
    {
      return (OrderablePartitioner<TSource>) new ChunkPartitioner<TSource>(source, nextChunkSizeFunc);
    }

    public static OrderablePartitioner<TSource> Create<TSource>(
      IEnumerable<TSource> source,
      int chunkSize)
    {
      return (OrderablePartitioner<TSource>) new ChunkPartitioner<TSource>(source, chunkSize);
    }

    public static OrderablePartitioner<TSource> Create<TSource>(
      IEnumerable<TSource> source,
      int minChunkSize,
      int maxChunkSize)
    {
      return (OrderablePartitioner<TSource>) new ChunkPartitioner<TSource>(source, minChunkSize, maxChunkSize);
    }
  }
}
