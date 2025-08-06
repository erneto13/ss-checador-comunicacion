using System;

namespace ChecadorComunicacion.Services;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Matricula { get; set; }
    public string TipoPersona { get; set; }
    public byte[] Huella { get; set; }
    public byte[] Foto { get; set; }

    public string FotoBase64 => Foto != null && Foto.Length > 0
        ? $"data:image/png;base64,{Convert.ToBase64String(Foto)}"
        : null;
}