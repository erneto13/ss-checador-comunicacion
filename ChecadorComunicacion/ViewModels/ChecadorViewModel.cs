using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

    public ChecadorViewModel()
    {
        _checadorService = new ChecadorService();
        _personaService = new PersonaService();

        InicializarReloj();
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

    public string NombreCompleto =>
        PersonaSeleccionada?.Nombre + PersonaSeleccionada?.Apellido ?? "Seleccione una persona";

    public string Matricula => PersonaSeleccionada?.Matricula ?? "";
    public string FechaUltimoChecador => UltimoChecador?.Fecha.ToString("dd/MM/yyyy") ?? "";
    public string HoraUltimoChecador => UltimoChecador?.Hora.ToString("HH:mm:ss") ?? "";
    public string TipoAccionAnterior => UltimoChecador?.TipoAccion ?? "Sin registros";

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
        FechaActual = ahora.ToString("dddd, dd 'de' MMMM 'de' yyyy");
    }

    private void ActualizarInformacionPersona()
    {
        if (PersonaSeleccionada == null)
        {
            UltimoChecador = null;
            ProximoTipoAccion = "";
        }
        else
        {
            UltimoChecador = _checadorService.ObtenerUltimoChecadorPorPersona(PersonaSeleccionada.Id);
            ProximoTipoAccion = _checadorService.DeterminarTipoAccion(PersonaSeleccionada.Id);
        }

        // Notificar cambios en las propiedades calculadas
        OnPropertyChanged(nameof(NombreCompleto));
        OnPropertyChanged(nameof(Matricula));
        OnPropertyChanged(nameof(FechaUltimoChecador));
        OnPropertyChanged(nameof(HoraUltimoChecador));
        OnPropertyChanged(nameof(TipoAccionAnterior));
    }

    public async Task RegistrarChecadorAsync()
    {
        if (PersonaSeleccionada == null) return;

        try
        {
            var nuevoChecador = _checadorService.RegistrarChecador(PersonaSeleccionada.Id);

            // Actualizar la información mostrada
            ActualizarInformacionPersona();

            // Aquí podrías mostrar un mensaje de éxito o hacer alguna animación
        }
        catch (Exception ex)
        {
            // Manejar errores - podrías mostrar un mensaje de error
            // Por ejemplo, usando un sistema de notificaciones
        }
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
}