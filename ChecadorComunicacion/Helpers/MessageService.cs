using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace ChecadorComunicacion.Helpers;

public static class MessageService
{
    public static async Task ShowErrorAsync(string message)
    {
        
        var messageBox = new Window
        {
            Title = "Error",
            Content = new TextBlock { Text = message },
            Height = 500,
            Width = 300,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        await messageBox.ShowDialog((Window)Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
    }
    
    
}