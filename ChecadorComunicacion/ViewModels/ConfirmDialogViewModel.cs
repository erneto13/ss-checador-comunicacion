using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChecadorComunicacion.ViewModels;

public partial class ConfirmDialogViewModel : DialogViewModel
{
    [ObservableProperty] private string _title = "Confirmación";
    [ObservableProperty] private string _message = "¿Estás seguro de esta acción?";
    [ObservableProperty] private string _confirmText = "Si";
    [ObservableProperty] private string _cancelText = "No";
    [ObservableProperty] private string _iconText = "\xe7ba";

    [ObservableProperty] private bool _confirmed;

    [RelayCommand]
    public void Confirm()
    {
        Confirmed = true;
        Close();
    }

    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }
}