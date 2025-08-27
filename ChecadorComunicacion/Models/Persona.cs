namespace ChecadorComunicacion.Models;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Matricula { get; set; }
    public string TipoPersona { get; set; }
    //public byte[] Huella { get; set; }
    public byte[] Foto { get; set; }
}