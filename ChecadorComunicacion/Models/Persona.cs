using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace ChecadorComunicacion.Models
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Matricula { get; set; }
        public string TipoPersona { get; set; }

        private string _rutaFoto;

        private static readonly string RutaImagenPorDefecto = Path.Combine("Assets", "default.png");

        public string RutaFoto
        {
            get
            {
                if (!string.IsNullOrEmpty(_rutaFoto))
                    return _rutaFoto;

                if (File.Exists(RutaImagenPorDefecto))
                    return RutaImagenPorDefecto;

                return null;
            }
            set => _rutaFoto = value;
        }

        [NotMapped] public bool TieneImagenPersonalizada => !string.IsNullOrEmpty(_rutaFoto);

        [NotMapped] public bool TieneImagenDisponible => !string.IsNullOrEmpty(RutaFoto) && File.Exists(RutaFoto);

        [NotMapped] public static string RutaImagenPorDefectoEstatica => RutaImagenPorDefecto;
    }
}