using System.Threading.Tasks;

namespace SimpleMVVM.Framework
{
    public interface IAsyncCommand
    {
        Task ExecuteAsync();
    }

    public interface IAsyncCommand<in TParam>
    {
        Task ExecuteAsync(TParam parameter);
    }
}