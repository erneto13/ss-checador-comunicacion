using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using System.IO;
using ChecadorComunicacion.Helpers;
using ChecadorComunicacion.Models;
using ChecadorComunicacion.Services;

namespace ChecadorComunicacion.ViewModels;

public class ChecadorViewModel : DialogViewModel, INotifyPropertyChanged
{
    private readonly ChecadorService _checadorService;
    private readonly PersonaService _personaService;
    private Timer _timer;

    private string _tiempoActual;
    private string _fechaActual;
    private Persona _personaSeleccionada;
    private Checador _ultimoChecador;
    private string _proximoTipoAccion;
    private string _matriculaInput;
    private string _mensajeEstado;
    private bool _mostrarDatos;

    private Bitmap _fotoPersona;
    private bool _tieneFoto;

    public ChecadorViewModel()
    {
        _checadorService = new ChecadorService();
        _personaService = new PersonaService();

        ProcesarMatriculaCommand = new RelayCommand(async () => await ProcesarMatriculaAsync());

        InicializarReloj();
        ReiniciarVista();
    }

    public string TiempoActual
    {
        get => _tiempoActual;
        set => SetProperty(ref _tiempoActual, value);
    }

    public string FechaActual
    {
        get => _fechaActual;
        set => SetProperty(ref _fechaActual, value);
    }

    public Persona PersonaSeleccionada
    {
        get => _personaSeleccionada;
        set
        {
            if (SetProperty(ref _personaSeleccionada, value))
            {
                ActualizarInformacionPersona();
            }
        }
    }

    public Checador UltimoChecador
    {
        get => _ultimoChecador;
        set => SetProperty(ref _ultimoChecador, value);
    }

    public string ProximoTipoAccion
    {
        get => _proximoTipoAccion;
        set => SetProperty(ref _proximoTipoAccion, value);
    }

    public string MatriculaInput
    {
        get => _matriculaInput;
        set => SetProperty(ref _matriculaInput, value);
    }

    public string MensajeEstado
    {
        get => _mensajeEstado;
        set => SetProperty(ref _mensajeEstado, value);
    }

    public bool MostrarDatos
    {
        get => _mostrarDatos;
        set => SetProperty(ref _mostrarDatos, value);
    }

    public Bitmap FotoPersona
    {
        get => _fotoPersona;
        set
        {
            _fotoPersona?.Dispose();
            _fotoPersona = value;
            OnPropertyChanged();
        }
    }

    public bool TieneFoto
    {
        get => _tieneFoto;
        set
        {
            _tieneFoto = value;
            OnPropertyChanged();
        }
    }

    public string NombreCompleto =>
        PersonaSeleccionada != null
            ? $"{PersonaSeleccionada.Nombre} {PersonaSeleccionada.Apellido}"
            : "Seleccione una persona";

    public string Matricula => PersonaSeleccionada?.Matricula ?? "";
    public string FechaUltimoChecador => UltimoChecador?.Fecha.ToString("dd/MM/yyyy") ?? "------";
    public string HoraUltimoChecador => UltimoChecador?.Hora.ToString("HH:mm:ss") ?? "------";
    public string TipoAccionAnterior => UltimoChecador?.TipoAccion ?? "Sin registros";

    public ICommand ProcesarMatriculaCommand { get; }

    private void InicializarReloj()
    {
        ActualizarTiempo();
        _timer = new Timer(OnTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void OnTimerElapsed(object state)
    {
        ActualizarTiempo();
    }

    private void ActualizarTiempo()
    {
        var ahora = DateTime.Now;
        TiempoActual = ahora.ToString("HH:mm:ss");
        FechaActual = ahora.ToString("dddd, dd 'de' MMMM 'de' yyyy",
            new System.Globalization.CultureInfo("es-ES"));
    }

    private void ActualizarInformacionPersona()
    {
        if (PersonaSeleccionada == null)
        {
            UltimoChecador = null;
            ProximoTipoAccion = "";
            LimpiarFoto();
        }
        else
        {
            UltimoChecador = _checadorService.ObtenerUltimoChecadorPorPersona(PersonaSeleccionada.Id);
            ProximoTipoAccion = _checadorService.DeterminarTipoAccion(PersonaSeleccionada.Id);

            CargarFotoPersona(PersonaSeleccionada.RutaFoto);
        }

        OnPropertyChanged(nameof(NombreCompleto));
        OnPropertyChanged(nameof(Matricula));
        OnPropertyChanged(nameof(FechaUltimoChecador));
        OnPropertyChanged(nameof(HoraUltimoChecador));
        OnPropertyChanged(nameof(TipoAccionAnterior));
    }

    private async Task ProcesarMatriculaAsync()
    {
        if (string.IsNullOrWhiteSpace(MatriculaInput))
        {
            MensajeEstado = "Ingrese una matrícula válida";
            return;
        }

        try
        {
            var persona = _personaService.ObtenerPersonaPorMatricula(MatriculaInput.Trim());

            if (persona == null)
            {
                MensajeEstado = "Matrícula no encontrada";
                MostrarDatos = false;
                LimpiarFoto();

                _ = Task.Delay(3000).ContinueWith(_ => ReiniciarVista());
                return;
            }

            PersonaSeleccionada = persona;
            MostrarDatos = true;

            var nuevoChecador = _checadorService.RegistrarChecador(PersonaSeleccionada.Id);

            ActualizarInformacionPersona();

            MensajeEstado = $"✓ {TipoAccionAnterior} registrada exitosamente";

            _ = Task.Delay(5000).ContinueWith(_ => ReiniciarVista());
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al procesar: {ex.Message}";
            LimpiarFoto();

            _ = Task.Delay(3000).ContinueWith(_ => ReiniciarVista());
        }
    }

    private void CargarFotoPersona(string fotoPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(fotoPath) && File.Exists(fotoPath))
            {
                using var stream = new FileStream(fotoPath, FileMode.Open, FileAccess.Read);
                FotoPersona = new Bitmap(stream);
                TieneFoto = true;
            }
            else
            {
                LimpiarFoto();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar foto: {ex.Message}");
            LimpiarFoto();
        }
    }

    private void LimpiarFoto()
    {
        FotoPersona?.Dispose();
        FotoPersona = null;
        TieneFoto = false;
    }

    private void ReiniciarVista()
    {
        PersonaSeleccionada = null;
        MatriculaInput = "";
        MensajeEstado = "";
        MostrarDatos = false;
        LimpiarFoto();
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value)) return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _timer?.Dispose();
        FotoPersona?.Dispose();
    }
}