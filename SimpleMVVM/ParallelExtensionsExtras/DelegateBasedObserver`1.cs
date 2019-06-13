// Decompiled with JetBrains decompiler
// Type: System.DelegateBasedObserver`1
// Assembly: ParallelExtensionsExtras, Version=1.2.3.0, Culture=neutral, PublicKeyToken=665f4d61f853b5a9
// MVID: 3E0D7B0F-D1DC-4BB8-9763-D6AFDAD2FBE9
// Assembly location: C:\Users\BMPickens\.nuget\packages\parallelextensionsextras\1.2.0\lib\net40\ParallelExtensionsExtras.dll

namespace System
{
  internal class DelegateBasedObserver<T> : IObserver<T>
  {
    private Action<T> _onNext;
    private Action<Exception> _onError;
    private Action _onCompleted;

    internal DelegateBasedObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
    {
      if (onNext == null)
        throw new ArgumentNullException(nameof (onNext));
      if (onError == null)
        throw new ArgumentNullException(nameof (onError));
      if (onCompleted == null)
        throw new ArgumentNullException(nameof (onCompleted));
      this._onNext = onNext;
      this._onError = onError;
      this._onCompleted = onCompleted;
    }

    public void OnCompleted()
    {
      this._onCompleted();
    }

    public void OnError(Exception error)
    {
      this._onError(error);
    }

    public void OnNext(T value)
    {
      this._onNext(value);
    }
  }
}
