using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChecadorComunicacion.Models;
using ChecadorComunicacion.Services;
using ChecadorComunicacion.Helpers; 
using OfficeOpenXml;
using Avalonia.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChecadorComunicacion.ViewModels
{
    public class ReportesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly ChecadorService _checadorService;
        private readonly PersonaService _personaService;

        private DateTimeOffset? _fechaInicio = DateTimeOffset.Now.Date.AddDays(-30);
        private DateTimeOffset? _fechaFin = DateTimeOffset.Now.Date;
        private string _tipoPersonaSeleccionado = "Todos";
        private bool _isLoading = false;
        private string _mensajeError = string.Empty;

        private ObservableCollection<Reporte> _Reportes = new();

        private int _totalRegistros;
        private int _personasUnicas;
        private double _totalHoras;
        private double _promedioHorasPorDia;

        public ReportesViewModel()
        {
            LogMessage("Constructor: Iniciando ViewModel");

            _checadorService = new ChecadorService();
            _personaService = new PersonaService();

            GenerarReporteCommand = new RelayCommand(async () => await GenerarReporteWrapper());
            ExportarCommand = new RelayCommand(async () => await ExportarExcelWrapper());
            LimpiarCommand = new RelayCommand(() => Limpiar());

            TiposPersona = new ObservableCollection<string> { "Todos", "Asesor", "Brigadista", "Administrativo" };

            LogMessage("Constructor: ViewModel inicializado correctamente");
        }

        private async Task GenerarReporteWrapper()
        {
            if (IsLoading) return;

            try
            {
                await GenerarReporte();
            }
            catch (Exception ex)
            {
                LogMessage($"GenerarReporteWrapper: Error - {ex.Message}");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    MensajeError = $"Error: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task ExportarExcelWrapper()
        {
            if (IsLoading) return;

            try
            {
                await ExportarExcel();
            }
            catch (Exception ex)
            {
                LogMessage($"ExportarExcelWrapper: Error - {ex.Message}");
                await Dispatcher.UIThread.InvokeAsync(() => { MensajeError = $"Error: {ex.Message}"; });
            }
        }

        private void LogMessage(string message)
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var isMainThread = Dispatcher.UIThread.CheckAccess();
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Thread {threadId} (UI: {isMainThread}): {message}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Thread {threadId} (UI: {isMainThread}): {message}");
        }

        #region Propiedades

        public DateTimeOffset? FechaInicio
        {
            get => _fechaInicio;
            set
            {
                LogMessage($"FechaInicio: Cambiando de {_fechaInicio} a {value}");
                SetProperty(ref _fechaInicio, value);
            }
        }

        public DateTimeOffset? FechaFin
        {
            get => _fechaFin;
            set
            {
                LogMessage($"FechaFin: Cambiando de {_fechaFin} a {value}");
                SetProperty(ref _fechaFin, value);
            }
        }

        public string TipoPersonaSeleccionado
        {
            get => _tipoPersonaSeleccionado;
            set
            {
                LogMessage($"TipoPersonaSeleccionado: Cambiando de {_tipoPersonaSeleccionado} a {value}");
                SetProperty(ref _tipoPersonaSeleccionado, value);
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                LogMessage($"IsLoading: Cambiando de {_isLoading} a {value}");
                SetProperty(ref _isLoading, value);
            }
        }

        public string MensajeError
        {
            get => _mensajeError;
            set
            {
                LogMessage($"MensajeError: Cambiando a {value}");
                SetProperty(ref _mensajeError, value);
            }
        }

        public ObservableCollection<Reporte> Reportes
        {
            get => _Reportes;
            set
            {
                LogMessage($"Reportes: Cambiando colección (count: {value?.Count ?? 0})");
                SetProperty(ref _Reportes, value);
            }
        }

        public ObservableCollection<string> TiposPersona { get; set; }

        public int TotalRegistros
        {
            get => _totalRegistros;
            set
            {
                LogMessage($"TotalRegistros: Cambiando de {_totalRegistros} a {value}");
                SetProperty(ref _totalRegistros, value);
            }
        }

        public int PersonasUnicas
        {
            get => _personasUnicas;
            set
            {
                LogMessage($"PersonasUnicas: Cambiando de {_personasUnicas} a {value}");
                SetProperty(ref _personasUnicas, value);
            }
        }

        public double TotalHoras
        {
            get => _totalHoras;
            set
            {
                LogMessage($"TotalHoras: Cambiando de {_totalHoras} a {value}");
                SetProperty(ref _totalHoras, value);
            }
        }

        public double PromedioHorasPorDia
        {
            get => _promedioHorasPorDia;
            set
            {
                LogMessage($"PromedioHorasPorDia: Cambiando de {_promedioHorasPorDia} a {value}");
                SetProperty(ref _promedioHorasPorDia, value);
            }
        }

        #endregion

        #region Comandos

        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand LimpiarCommand { get; }

        #endregion

        #region Métodos

        private async Task GenerarReporte()
        {
            LogMessage("GenerarReporte: Método iniciado");

            LogMessage("GenerarReporte: Limpiando mensaje de error");
            await Dispatcher.UIThread.InvokeAsync(() => MensajeError = string.Empty);

            LogMessage("GenerarReporte: Validando filtros");
            if (!ValidarFiltros())
            {
                LogMessage("GenerarReporte: Validación de filtros falló");
                return;
            }

            LogMessage("GenerarReporte: Activando loading");
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = true);

            try
            {
                LogMessage("GenerarReporte: Iniciando obtención de datos en Task.Run");
                var fechaInicioCapturada = FechaInicio.Value;
                var fechaFinCapturada = FechaFin.Value;
                var tipoPersonaCapturado = TipoPersonaSeleccionado;

                var datos = await Task.Run(() =>
                {
                    LogMessage("Task.Run: Iniciando obtención de datos");
                    try
                    {
                        var resultado = ObtenerDatosReporte(fechaInicioCapturada, fechaFinCapturada,
                            tipoPersonaCapturado);
                        LogMessage($"Task.Run: Datos obtenidos exitosamente. Count: {resultado.Count}");
                        return resultado;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Task.Run: Error al obtener datos - {ex.Message}");
                        throw;
                    }
                });

                LogMessage("GenerarReporte: Datos obtenidos, actualizando colección en UI thread");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Reportes.Clear();
                    LogMessage("GenerarReporte: Colección limpiada");

                    foreach (var item in datos)
                    {
                        Reportes.Add(item);
                    }

                    LogMessage($"GenerarReporte: {datos.Count} elementos agregados a la colección");

                    LogMessage("GenerarReporte: Calculando estadísticas");
                    CalcularEstadisticas();
                    LogMessage("GenerarReporte: Estadísticas calculadas");
                });
            }
            catch (Exception ex)
            {
                LogMessage($"GenerarReporte: Error capturado - {ex.Message}");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    MensajeError = $"Error al generar reporte: {ex.Message}";
                });
            }
            finally
            {
                LogMessage("GenerarReporte: Desactivando loading");
                await Dispatcher.UIThread.InvokeAsync(() => { IsLoading = false; });
                LogMessage("GenerarReporte: Método finalizado");
            }
        }

        private bool ValidarFiltros()
        {
            LogMessage("ValidarFiltros: Iniciando validación");

            if (FechaInicio == null || FechaFin == null)
            {
                LogMessage("ValidarFiltros: Fechas nulas");
                Dispatcher.UIThread.Post(() => MensajeError = "Debe seleccionar fechas de inicio y fin");
                return false;
            }

            if (FechaInicio > FechaFin)
            {
                LogMessage("ValidarFiltros: Fecha inicio mayor que fecha fin");
                Dispatcher.UIThread.Post(() => MensajeError = "La fecha de inicio no puede ser mayor a la fecha fin");
                return false;
            }

            LogMessage("ValidarFiltros: Validación exitosa");
            return true;
        }

        private List<Reporte> ObtenerDatosReporte(DateTimeOffset fechaInicio, DateTimeOffset fechaFin,
            string tipoPersona)
        {
            LogMessage(
                $"ObtenerDatosReporte: Iniciado con fechas {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}, tipo: {tipoPersona}");

            var fechaInicioOnly = DateOnly.FromDateTime(fechaInicio.DateTime);
            var fechaFinOnly = DateOnly.FromDateTime(fechaFin.DateTime);

            LogMessage("ObtenerDatosReporte: Obteniendo checadores del servicio");
            var checadores = _checadorService.ObtenerChecadoresPorRangoFecha(fechaInicioOnly, fechaFinOnly);
            LogMessage($"ObtenerDatosReporte: {checadores.Count} checadores obtenidos");

            if (tipoPersona != "Todos")
            {
                LogMessage($"ObtenerDatosReporte: Filtrando por tipo de persona: {tipoPersona}");
                checadores = checadores.Where(c => c.Persona.TipoPersona == tipoPersona).ToList();
                LogMessage($"ObtenerDatosReporte: {checadores.Count} checadores después del filtro");
            }

            var reportesList = new List<Reporte>();

            LogMessage("ObtenerDatosReporte: Agrupando checadores");
            var gruposPorPersonaYFecha = checadores
                .GroupBy(c => new { c.PersonaId, c.Fecha })
                .OrderBy(g => g.Key.Fecha)
                .ThenBy(g => g.First().Persona.Nombre);

            LogMessage($"ObtenerDatosReporte: {gruposPorPersonaYFecha.Count()} grupos creados");

            foreach (var grupo in gruposPorPersonaYFecha)
            {
                var persona = grupo.First().Persona;
                var registrosDelDia = grupo.OrderBy(c => c.Hora).ToList();

                var entrada = registrosDelDia.FirstOrDefault(r => r.TipoAccion == "Entrada");
                var salida = registrosDelDia.FirstOrDefault(r => r.TipoAccion == "Salida");

                var horasTrabajadas = CalcularHorasTrabajadas(entrada, salida);

                reportesList.Add(new Reporte
                {
                    Fecha = grupo.Key.Fecha,
                    Matricula = persona.Matricula,
                    Nombre = $"{persona.Nombre} {persona.Apellido}",
                    TipoPersona = persona.TipoPersona,
                    HoraEntrada = entrada?.Hora.ToString("HH:mm") ?? "Sin registro",
                    HoraSalida = salida?.Hora.ToString("HH:mm") ?? "Sin registro",
                    HorasTrabajadas = horasTrabajadas
                });
            }

            LogMessage($"ObtenerDatosReporte: {reportesList.Count} reportes creados");
            return reportesList;
        }

        private double CalcularHorasTrabajadas(Checador entrada, Checador salida)
        {
            if (entrada == null || salida == null)
                return 0;

            var tiempoEntrada = entrada.Hora;
            var tiempoSalida = salida.Hora;

            if (tiempoSalida < tiempoEntrada)
            {
                tiempoSalida = tiempoSalida.Add(TimeSpan.FromHours(24));
            }

            var diferencia = tiempoSalida.ToTimeSpan() - tiempoEntrada.ToTimeSpan();
            return diferencia.TotalHours;
        }

        private void CalcularEstadisticas()
        {
            LogMessage("CalcularEstadisticas: Iniciando cálculo");

            TotalRegistros = Reportes.Count;
            PersonasUnicas = Reportes.Select(r => r.Matricula).Distinct().Count();
            TotalHoras = Math.Round(Reportes.Sum(r => r.HorasTrabajadas), 2);

            if (FechaInicio.HasValue && FechaFin.HasValue)
            {
                var dias = (FechaFin.Value.DateTime - FechaInicio.Value.DateTime).Days + 1;
                PromedioHorasPorDia = dias > 0 ? Math.Round(TotalHoras / dias, 2) : 0;
            }

            LogMessage(
                $"CalcularEstadisticas: Totales - Registros: {TotalRegistros}, Personas: {PersonasUnicas}, Horas: {TotalHoras}");
        }

        private async Task ExportarExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Eee");
            
            LogMessage("ExportarExcel: Método iniciado");

            if (Reportes.Count == 0)
            {
                LogMessage("ExportarExcel: No hay datos para exportar");
                await Dispatcher.UIThread.InvokeAsync(() =>
                    MensajeError = "No hay datos para exportar. Genere un reporte primero.");
                return;
            }

            LogMessage("ExportarExcel: Copiando datos para exportación");
            var datosParaExportar = new
            {
                Reportes = Reportes.ToList(),
                TotalRegistros = this.TotalRegistros,
                PersonasUnicas = this.PersonasUnicas,
                TotalHoras = this.TotalHoras,
                PromedioHorasPorDia = this.PromedioHorasPorDia
            };

            LogMessage("ExportarExcel: Iniciando generación de Excel en Task.Run");
            var archivoGenerado = await Task.Run(() =>
            {
                LogMessage("Task.Run Excel: Iniciando generación");

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Reporte de Asistencia");

                worksheet.Cells[1, 1].Value = "Fecha";
                worksheet.Cells[1, 2].Value = "Matrícula";
                worksheet.Cells[1, 3].Value = "Nombre";
                worksheet.Cells[1, 4].Value = "Tipo";
                worksheet.Cells[1, 5].Value = "Entrada";
                worksheet.Cells[1, 6].Value = "Salida";
                worksheet.Cells[1, 7].Value = "Horas Trabajadas";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                for (int i = 0; i < datosParaExportar.Reportes.Count; i++)
                {
                    var item = datosParaExportar.Reportes[i];
                    int row = i + 2;

                    worksheet.Cells[row, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 2].Value = item.Matricula;
                    worksheet.Cells[row, 3].Value = item.Nombre;
                    worksheet.Cells[row, 4].Value = item.TipoPersona;
                    worksheet.Cells[row, 5].Value = item.HoraEntrada;
                    worksheet.Cells[row, 6].Value = item.HoraSalida;
                    worksheet.Cells[row, 7].Value = item.HorasTrabajadas;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "0.00";
                }

                int statsRow = datosParaExportar.Reportes.Count + 4;
                worksheet.Cells[statsRow, 1].Value = "ESTADÍSTICAS";
                worksheet.Cells[statsRow, 1].Style.Font.Bold = true;

                worksheet.Cells[statsRow + 1, 1].Value = "Total Registros:";
                worksheet.Cells[statsRow + 1, 2].Value = datosParaExportar.TotalRegistros;

                worksheet.Cells[statsRow + 2, 1].Value = "Personas Únicas:";
                worksheet.Cells[statsRow + 2, 2].Value = datosParaExportar.PersonasUnicas;

                worksheet.Cells[statsRow + 3, 1].Value = "Total Horas:";
                worksheet.Cells[statsRow + 3, 2].Value = datosParaExportar.TotalHoras;

                worksheet.Cells[statsRow + 4, 1].Value = "Promedio Horas/Día:";
                worksheet.Cells[statsRow + 4, 2].Value = datosParaExportar.PromedioHorasPorDia;

                worksheet.Cells.AutoFitColumns();

                var fileName = $"Reporte_Asistencia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                File.WriteAllBytes(filePath, package.GetAsByteArray());

                LogMessage($"Task.Run Excel: Archivo generado en {filePath}");
                return filePath;
            });

            LogMessage("ExportarExcel: Actualizando mensaje de éxito");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                MensajeError = $"Archivo exportado exitosamente: {archivoGenerado}";
            });

            LogMessage("ExportarExcel: Método finalizado");
        }

        private void Limpiar()
        {
            LogMessage("Limpiar: Método iniciado");

            FechaInicio = null;
            FechaFin = null;
            TipoPersonaSeleccionado = "Todos";
            Reportes.Clear();
            MensajeError = string.Empty;
            TotalRegistros = 0;
            PersonasUnicas = 0;
            TotalHoras = 0;
            PromedioHorasPorDia = 0;

            LogMessage("Limpiar: Método finalizado");
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}