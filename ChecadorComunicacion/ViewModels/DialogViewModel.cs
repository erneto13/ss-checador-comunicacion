using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChecadorComunicacion.ViewModels;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isDialogOpen;

    protected TaskCompletionSource CloseTask = new();

    public async Task WaitAsync()
    {
        await CloseTask.Task;
    }

    public void Open()
    {
        if (CloseTask.Task.IsCompleted)
            CloseTask = new TaskCompletionSource();

        IsDialogOpen = true;
    }

    protected void Close()
    {
        IsDialogOpen = false;
        CloseTask.TrySetResult();
    }
}