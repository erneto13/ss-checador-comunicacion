using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ChecadorComunicacion.ViewModels;
using FluentAvalonia.UI.Controls;

namespace ChecadorComunicacion;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.FindControl<NavigationView>("NavigationView") is NavigationView navView)
        {
            navView.SelectionChanged += NavigationView_SelectionChanged;
        }
    }

    private void NavigationView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem item &&
            item.Tag is string tagName &&
            DataContext is MainWindowViewModel viewModel)
        {
            var listItem = viewModel.Items.FirstOrDefault(x => x.ModelType.Name == tagName);
            if (listItem != null)
            {
                viewModel.SelectedListItem = listItem;
            }
        }
    }

    private void CloseApp_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeApp_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}