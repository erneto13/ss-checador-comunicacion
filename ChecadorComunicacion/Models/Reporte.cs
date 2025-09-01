using System;

namespace ChecadorComunicacion.Models;

public class Reporte
{
    public DateOnly Fecha { get; set; }
    public string Matricula { get; set; }
    public string Nombre { get; set; }
    public string TipoPersona { get; set; }
    public string HoraEntrada { get; set; }
    public string HoraSalida { get; set; }
    public double HorasTrabajadas { get; set; }
}