using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChecadorComunicacion.Helpers;
using ChecadorComunicacion.Services;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChecadorComunicacion.Models;
using Avalonia.Controls;
using Avalonia;

namespace ChecadorComunicacion.ViewModels;

public class PersonasViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly PersonaService _service = new();
    private TopLevel _topLevel;
    private byte[] _fotoTemporalBytes;

    private static readonly string RutaImagenPorDefecto = Persona.RutaImagenPorDefectoEstatica;

    public ObservableCollection<Persona> Personas { get; } = new();

    public ICommand AgregarCommand { get; }
    public ICommand ActualizarCommand { get; }
    public ICommand EliminarCommand { get; }
    public ICommand LimpiarCommand { get; }
    public ICommand SeleccionarImagenCommand { get; }
    public ICommand LimpiarImagenCommand { get; }

    public PersonasViewModel()
    {
        AgregarCommand = new RelayCommand(Agregar);
        ActualizarCommand = new RelayCommand(Actualizar);
        EliminarCommand = new RelayCommand(Eliminar);
        LimpiarCommand = new RelayCommand(Limpiar);
        SeleccionarImagenCommand = new RelayCommand(async () => await SeleccionarImagen());
        LimpiarImagenCommand = new RelayCommand(LimpiarImagen);

        CargarPersonas();
        Limpiar();
    }

    private byte[] ObtenerBytesImagenPorDefecto()
    {
        try
        {
            if (File.Exists(RutaImagenPorDefecto))
            {
                return File.ReadAllBytes(RutaImagenPorDefecto);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener bytes de imagen por defecto: {ex.Message}");
        }

        return null;
    }

    private Persona _personaSeleccionada;

    public Persona PersonaSeleccionada
    {
        get => _personaSeleccionada;
        set
        {
            if (_personaSeleccionada != value)
            {
                _personaSeleccionada = value;
                OnPropertyChanged();
                if (value is not null)
                {
                    Nombre = value.Nombre;
                    Apellido = value.Apellido;
                    Matricula = value.Matricula;
                    TipoPersona = value.TipoPersona;
                    RutaFoto = value.RutaFoto;
                    _fotoTemporalBytes = null;
                }
                else
                {
                    Limpiar();
                }
            }
        }
    }

    private string _nombre;

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    private string _apellido;

    public string Apellido
    {
        get => _apellido;
        set => SetProperty(ref _apellido, value);
    }

    private string _matricula;

    public string Matricula
    {
        get => _matricula;
        set => SetProperty(ref _matricula, value);
    }

    private string _tipoPersona;

    public string TipoPersona
    {
        get => _tipoPersona;
        set => SetProperty(ref _tipoPersona, value);
    }

    public List<string> TiposPersona { get; } = new()
    {
        "Asesor",
        "Brigadista",
        "Administrativo"
    };

    private string _RutaFoto;
    private Bitmap _fotoPreview;

    public string RutaFoto
    {
        get => _RutaFoto;
        set
        {
            _RutaFoto = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TieneFoto));
            ActualizarPreviewFoto();
        }
    }

    public bool TieneFoto => !string.IsNullOrEmpty(RutaFoto);

    public Bitmap FotoPreview
    {
        get => _fotoPreview;
        set => SetProperty(ref _fotoPreview, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void ActualizarPreviewFoto()
    {
        try
        {
            FotoPreview?.Dispose();
            FotoPreview = null;

            string rutaAMostrar = null;

            if (!string.IsNullOrEmpty(RutaFoto) && File.Exists(RutaFoto))
            {
                rutaAMostrar = RutaFoto;
            }
            else if (File.Exists(RutaImagenPorDefecto))
            {
                rutaAMostrar = RutaImagenPorDefecto;
            }

            if (!string.IsNullOrEmpty(rutaAMostrar))
            {
                using var stream = new FileStream(rutaAMostrar, FileMode.Open, FileAccess.Read);
                FotoPreview = new Bitmap(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear preview de imagen: {ex.Message}");
            FotoPreview = null;
        }
    }

    private void CargarPersonas()
    {
        try
        {
            Personas.Clear();
            var personas = _service.ObtenerPersonas();
            foreach (var persona in personas)
            {
                Personas.Add(persona);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error al cargar personas: {ex.Message}");
        }
    }

    public void Limpiar()
    {
        Nombre = string.Empty;
        Apellido = string.Empty;
        Matricula = string.Empty;
        TipoPersona = null;
        RutaFoto = null;
        _fotoTemporalBytes = null;
        PersonaSeleccionada = null;

        ActualizarPreviewFoto();
    }

    public void LimpiarImagen()
    {
        RutaFoto = null;
        _fotoTemporalBytes = null;
        ActualizarPreviewFoto();
    }

    public bool Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ShowError("El nombre es obligatorio");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Apellido))
        {
            ShowError("El apellido es obligatorio");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Matricula))
        {
            ShowError("La matrícula es obligatoria");
            return false;
        }

        if (string.IsNullOrWhiteSpace(TipoPersona))
        {
            ShowError("El tipo de persona es obligatorio");
            return false;
        }

        return true;
    }

    public void SetTopLevel(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    private async void ShowError(string message)
    {
        if (_topLevel == null) 
        {
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
            return;
        }

        try
        {
            var dialog = new Window
            {
                Title = "Error",
                Content = new TextBlock { Text = message, Margin = new Thickness(20) },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            await dialog.ShowDialog(_topLevel as Window);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al mostrar diálogo: {ex.Message}");
        }
    }

    private async void ShowMessage(string message)
    {
        if (_topLevel == null) 
        {
            System.Diagnostics.Debug.WriteLine($"Mensaje: {message}");
            return;
        }

        try
        {
            var dialog = new Window
            {
                Title = "Información",
                Content = new TextBlock { Text = message, Margin = new Thickness(20) },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            await dialog.ShowDialog(_topLevel as Window);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al mostrar diálogo: {ex.Message}");
        }
    }

    public void Agregar()
    {
        if (!Validar()) return;

        try
        {
            var nuevaPersona = new Persona
            {
                Nombre = Nombre.Trim(),
                Apellido = Apellido.Trim(),
                Matricula = Matricula.Trim(),
                TipoPersona = TipoPersona.Trim(),
                RutaFoto = ""
            };

            _service.AgregarPersona(nuevaPersona, _fotoTemporalBytes);

            CargarPersonas();
            Limpiar();
            ShowMessage("Persona agregada correctamente");
        }
        catch (Exception ex)
        {
            ShowError($"Error al agregar persona: {ex.Message}");
        }
    }

    public void Actualizar()
    {
        if (PersonaSeleccionada == null)
        {
            ShowError("Seleccione una persona para editar");
            return;
        }

        if (!Validar()) return;

        try
        {
            PersonaSeleccionada.Nombre = Nombre.Trim();
            PersonaSeleccionada.Apellido = Apellido.Trim();
            PersonaSeleccionada.Matricula = Matricula.Trim();
            PersonaSeleccionada.TipoPersona = TipoPersona.Trim();

            _service.ActualizarPersona(PersonaSeleccionada, _fotoTemporalBytes);

            CargarPersonas();
            Limpiar();
            ShowMessage("Persona actualizada correctamente");
        }
        catch (Exception ex)
        {
            ShowError($"Error al actualizar persona: {ex.Message}");
        }
    }

    public void Eliminar()
    {
        if (PersonaSeleccionada == null)
        {
            ShowError("Seleccione una persona para eliminar");
            return;
        }

        try
        {
            _service.EliminarPersona(PersonaSeleccionada);
            CargarPersonas();
            Limpiar();
            ShowMessage("Persona eliminada correctamente");
        }
        catch (Exception ex)
        {
            ShowError($"Error al eliminar persona: {ex.Message}");
        }
    }

    public async Task SeleccionarImagen()
    {
        try
        {
            if (_topLevel == null) return;

            var fileTypes = new FilePickerFileType[]
            {
                new("Imágenes")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
                    MimeTypes = new[] { "image/*" }
                }
            };

            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Seleccionar imagen",
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            });

            if (files.Count > 0)
            {
                var selectedFile = files[0];

                await using var stream = await selectedFile.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                _fotoTemporalBytes = memoryStream.ToArray();

                var tempPath = Path.GetTempFileName() + Path.GetExtension(selectedFile.Name);
                File.WriteAllBytes(tempPath, _fotoTemporalBytes);
                RutaFoto = tempPath;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al seleccionar imagen: {ex.Message}");
            ShowError($"Error al cargar la imagen: {ex.Message}");
        }
    }
}