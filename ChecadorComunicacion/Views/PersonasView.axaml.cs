using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChecadorComunicacion.ViewModels;

namespace ChecadorComunicacion.Views;

public partial class PersonasView : UserControl
{
    public PersonasView()
    {
        InitializeComponent();
        DataContext = new PersonasViewModel();
    }
    
    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        if (DataContext is PersonasViewModel viewModel)
        {
            viewModel.SetTopLevel(TopLevel.GetTopLevel(this));
        }
    }
}