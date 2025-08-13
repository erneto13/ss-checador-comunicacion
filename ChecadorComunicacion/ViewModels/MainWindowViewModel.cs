using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChecadorComunicacion.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isPaneOpen = true;

    [ObservableProperty] private ViewModelBase _currentPage = new ChecadorViewModel();

    [ObservableProperty] private ListItemTemplate? _selectedListItem;

    [ObservableProperty] private DialogViewModel _dialogViewModel = new ChecadorViewModel { IsDialogOpen = true };

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
        var instance = Activator.CreateInstance(value.ModelType);
        if (instance is null) return;
        CurrentPage = (ViewModelBase)instance;
    }

    public ObservableCollection<ListItemTemplate> Items { get; } = new()
    {
        new ListItemTemplate(typeof(ChecadorViewModel), "FingerprintRegular"),
        new ListItemTemplate(typeof(PersonaViewModel), "PeopleTeamAddRegular"),
        new ListItemTemplate(typeof(ReportesViewModel), "ReportDocumentRegular"),
    };

    [RelayCommand]
    public void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(Type type, string iconKey)
    {
        ModelType = type;
        Label = type.Name.Replace("ViewModel", "");

        Application.Current!.TryFindResource(iconKey, out var res);
        ListItemIcon = (StreamGeometry)res!;
    }

    public string Label { get; set; }
    public Type ModelType { get; set; }
    public StreamGeometry ListItemIcon { get; }
}