using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ChecadorComunicacion.ViewModels;
using FluentAvalonia.UI.Controls;

namespace ChecadorComunicacion;

public partial class MainWindow : Window
{
    private double _zoom = 1.0;

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

    private void OpenImageModal(object? sender, PointerPressedEventArgs e)
    {
        _zoom = 1.0;

        if (FullImage.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = _zoom;
            scale.ScaleY = _zoom;
        }

        ImageOverlay.IsVisible = true;
    }

    private void OnZoomImage(object? sender, PointerWheelEventArgs e)
    {
        if (FullImage.RenderTransform is ScaleTransform scale)
        {
            if (e.Delta.Y > 0)
                _zoom += 0.1;
            else if (e.Delta.Y < 0 && _zoom > 0.2)
                _zoom -= 0.1;

            scale.ScaleX = _zoom;
            scale.ScaleY = _zoom;
        }
    }

    private void CloseImageModal(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Grid)
            ImageOverlay.IsVisible = false;
    }

}