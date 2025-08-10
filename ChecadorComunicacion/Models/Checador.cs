using System;

namespace ChecadorComunicacion.Services;

public class Checador
{
    public int Id { get; set; }
    public string TipoAccion { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public int PersonaId { get; set; }
    public Persona Persona { get; set; }
}