using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChecadorComunicacion.Data;
using Microsoft.EntityFrameworkCore;

namespace ChecadorComunicacion;

public partial class App : Application
{
    
    private readonly AppDbContext _db = new();
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _db.Database.Migrate();
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}