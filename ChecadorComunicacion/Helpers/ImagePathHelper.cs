using System;
using System.IO;
using ChecadorComunicacion.Models;
using Path = System.IO.Path;

namespace ChecadorComunicacion.Helpers;

public static class ImagePathHelper
{
    private static readonly string BaseImagePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "ChecadorComunicacion", "FotosPersonas");

    static ImagePathHelper()
    {
        if (!Directory.Exists(BaseImagePath))
        {
            Directory.CreateDirectory(BaseImagePath);
        }
    }

    public static string GetImagePath(string matricula, string extension = ".jpg")
    {
        return Path.Combine(BaseImagePath, $"{matricula}{extension}");
    }

    public static string GetImagePathForPersona(Persona persona, string extension = ".jpg")
    {
        return GetImagePath(persona.Matricula, extension);
    }

    public static void EnsureImageDirectoryExists()
    {
        if (!Directory.Exists(BaseImagePath))
        {
            Directory.CreateDirectory(BaseImagePath);
        }
    }
}