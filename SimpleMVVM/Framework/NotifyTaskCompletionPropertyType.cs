namespace SimpleMVVM.Framework
{
    public enum NotifyTaskCompletionPropertyType
    {
        None = 0,
        Status = 1,
        IsCompleted = 2,
        IsNotCompleted = 3,
        IsCancelled = 4,
        IsFaulted = 5,
        Exception = 6,
        InnerException = 7,
        ErrorMessage = 8,
        IsSuccessfullyCompleted = 9,
        Result = 10
    }
}
