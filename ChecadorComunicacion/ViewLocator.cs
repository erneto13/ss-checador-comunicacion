using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ChecadorComunicacion.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ChecadorComunicacion;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control?>> _locator = new();

    public Control Build(object? data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type);
        }

        return new TextBlock { Text = $"Not Found: {name}" };
    }

    public bool Match(object? data) => data is ViewModelBase or DialogViewModel;

    private void RegisterViewFactory<TViewModel, TView>()
        where TViewModel : class
        where TView : Control
        => _locator.Add(
            typeof(TViewModel),
            Design.IsDesignMode
                ? Activator.CreateInstance<TView>
                : Ioc.Default.GetService<TView>);
}