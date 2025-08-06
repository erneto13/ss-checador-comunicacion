using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChecadorComunicacion.Helpers;
using ChecadorComunicacion.Services;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChecadorComunicacion.ViewModels;

public class PersonaViewModel : INotifyPropertyChanged
{
    private readonly PersonaService _service = new();

    public ObservableCollection<Persona> Personas { get; } = new();

    public ICommand AgregarCommand { get; }
    public ICommand ActualizarCommand { get; }
    public ICommand EliminarCommand { get; }
    public ICommand LimpiarCommand { get; }
    public ICommand SeleccionarImagenCommand { get; }
    public ICommand GenerarHuellaCommand { get; }

    public PersonaViewModel()
    {
        AgregarCommand = new RelayCommand(Agregar);
        ActualizarCommand = new RelayCommand(Actualizar);
        EliminarCommand = new RelayCommand(Eliminar);
        LimpiarCommand = new RelayCommand(Limpiar);
        SeleccionarImagenCommand = new RelayCommand(async () => await SeleccionarImagen());
        GenerarHuellaCommand = new RelayCommand(GenerarHuellaAleatoria);

        CargarPersonas();
        Limpiar();
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
                if (value != null)
                {
                    Nombre = value.Nombre;
                    Apellido = value.Apellido;
                    Matricula = value.Matricula;
                    TipoPersona = value.TipoPersona;
                    Huella = value.Huella;
                    Foto = value.Foto;
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
        set
        {
            _nombre = value;
            OnPropertyChanged();
        }
    }

    private string _apellido;

    public string Apellido
    {
        get => _apellido;
        set
        {
            _apellido = value;
            OnPropertyChanged();
        }
    }

    private string _matricula;

    public string Matricula
    {
        get => _matricula;
        set
        {
            _matricula = value;
            OnPropertyChanged();
        }
    }

    private string _tipoPersona;

    public string TipoPersona
    {
        get => _tipoPersona;
        set
        {
            _tipoPersona = value;
            OnPropertyChanged();
        }
    }

    private byte[] _huella;

    public byte[] Huella
    {
        get => _huella;
        set
        {
            _huella = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HuellaTexto));
        }
    }

    public string HuellaTexto
    {
        get => Huella != null ? Convert.ToBase64String(Huella) : string.Empty;
    }

    private byte[] _foto;

    public byte[] Foto
    {
        get => _foto;
        set
        {
            _foto = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TieneFoto));
            OnPropertyChanged(nameof(FotoBase64));
        }
    }

    public bool TieneFoto => Foto != null && Foto.Length > 0;

    public string FotoBase64 => TieneFoto ? $"data:image/png;base64,{Convert.ToBase64String(Foto)}" : null;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void CargarPersonas()
    {
        Personas.Clear();
        var personas = _service.ObtenerPersonas();
        foreach (var persona in personas)
        {
            Personas.Add(persona);
        }
    }

    public void Limpiar()
    {
        Nombre = string.Empty;
        Apellido = string.Empty;
        Matricula = string.Empty;
        TipoPersona = string.Empty;
        Huella = null;
        Foto = null;
        PersonaSeleccionada = null;
    }

    public bool Validar() =>
        !string.IsNullOrWhiteSpace(Nombre) &&
        !string.IsNullOrWhiteSpace(Apellido) &&
        !string.IsNullOrWhiteSpace(Matricula) &&
        !string.IsNullOrWhiteSpace(TipoPersona) &&
        Huella != null &&
        Foto != null;

    public void Agregar()
    {
        if (!Validar()) return;

        var nuevaPersona = new Persona
        {
            Nombre = Nombre.Trim(),
            Apellido = Apellido.Trim(),
            Matricula = Matricula.Trim(),
            TipoPersona = TipoPersona.Trim(),
            Huella = Huella,
            Foto = Foto
        };

        _service.AgregarPersona(nuevaPersona);
        CargarPersonas();
        Limpiar();
    }

    public void Actualizar()
    {
        if (PersonaSeleccionada == null || !Validar()) return;

        PersonaSeleccionada.Nombre = Nombre.Trim();
        PersonaSeleccionada.Apellido = Apellido.Trim();
        PersonaSeleccionada.Matricula = Matricula.Trim();
        PersonaSeleccionada.TipoPersona = TipoPersona.Trim();
        PersonaSeleccionada.Huella = Huella;
        PersonaSeleccionada.Foto = Foto;

        _service.ActualizarPersona(PersonaSeleccionada);
        CargarPersonas();
        Limpiar();
    }

    public void Eliminar()
    {
        if (PersonaSeleccionada == null) return;

        _service.EliminarPersona(PersonaSeleccionada);
        CargarPersonas();
        Limpiar();
    }

    public async Task SeleccionarImagen()
    {
        try
        {
            var topLevel =
                Avalonia.Application.Current?.ApplicationLifetime is
                    Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            if (topLevel == null) return;

            var fileTypes = new FilePickerFileType[]
            {
                new("Imágenes")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
                    MimeTypes = new[] { "image/*" }
                }
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Seleccionar imagen",
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            });

            if (files.Count > 0)
            {
                var selectedFile = files[0];

                using (var stream = await selectedFile.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        Foto = memoryStream.ToArray();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al seleccionar imagen: {ex.Message}");
        }
    }

    public void GenerarHuellaAleatoria()
    {
        try
        {
            var random = new Random();
            var huellaData = new byte[256]; 
            random.NextBytes(huellaData);

            for (int i = 0; i < huellaData.Length; i += 8)
            {
                if (i + 8 < huellaData.Length)
                {
                    huellaData[i] = (byte)(random.Next(0, 255));
                    huellaData[i + 1] = (byte)(random.Next(0, 255));
                    huellaData[i + 2] = (byte)(random.Next(0, 200));
                    huellaData[i + 3] = (byte)(random.Next(0, 200));
                    huellaData[i + 4] = (byte)(random.Next(0, 360));
                    huellaData[i + 5] = (byte)(random.Next(50, 100));
                }
            }

            Huella = huellaData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al generar huella: {ex.Message}");
        }
    }
}