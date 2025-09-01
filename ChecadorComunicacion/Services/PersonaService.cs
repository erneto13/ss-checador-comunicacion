using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ChecadorComunicacion.Data;
using ChecadorComunicacion.Models;
using ChecadorComunicacion.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ChecadorComunicacion.Services;

public class PersonaService
{
    private readonly AppDbContext _context = new AppDbContext();

    public List<Persona> ObtenerPersonas(bool incluirFotos = true)
    {
        return _context.Personas.AsNoTracking().ToList();
    }

    public void AgregarPersona(Persona persona, byte[] imagenBytes = null)
    {
        try
        {
            var existeMatricula = _context.Personas
                .AsNoTracking()
                .Any(p => p.Matricula == persona.Matricula);

            if (existeMatricula)
            {
                throw new InvalidOperationException($"Ya existe una persona con la matrícula {persona.Matricula}");
            }

            if (imagenBytes != null && imagenBytes.Length > 0)
            {
                GuardarFotoPersona(persona, imagenBytes);
            }

            _context.Personas.Add(persona);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            if (persona.TieneImagenPersonalizada && !string.IsNullOrEmpty(persona.RutaFoto) && File.Exists(persona.RutaFoto))
            {
                try
                {
                    File.Delete(persona.RutaFoto);
                }
                catch { }
            }

            throw new Exception($"Error al agregar persona: {ex.Message}", ex);
        }
    }

    public void ActualizarPersona(Persona persona, byte[] imagenBytes = null)
    {
        try
        {
            var existeMatricula = _context.Personas
                .AsNoTracking()
                .Any(p => p.Matricula == persona.Matricula && p.Id != persona.Id);

            if (existeMatricula)
            {
                throw new InvalidOperationException($"Ya existe otra persona con la matrícula {persona.Matricula}");
            }

            var personaExistente = _context.Personas.AsNoTracking().FirstOrDefault(p => p.Id == persona.Id);
            
            if (imagenBytes != null && imagenBytes.Length > 0)
            {
                if (personaExistente?.TieneImagenPersonalizada == true)
                {
                    var rutaAnterior = personaExistente.RutaFoto;
                    if (!string.IsNullOrEmpty(rutaAnterior) && File.Exists(rutaAnterior))
                    {
                        try
                        {
                            File.Delete(rutaAnterior);
                        }
                        catch { }
                    }
                }

                GuardarFotoPersona(persona, imagenBytes);
            }
            else if (personaExistente != null)
            {
                persona.RutaFoto = personaExistente.TieneImagenPersonalizada ? 
                    personaExistente.RutaFoto : null;
            }

            _context.Update(persona);
            _context.Entry(persona).State = EntityState.Modified;
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar persona: {ex.Message}", ex);
        }
    }

    public void EliminarPersona(Persona persona)
    {
        try
        {
            var entidad = _context.Personas.Find(persona.Id);
            if (entidad == null)
            {
                throw new InvalidOperationException("La persona no existe o ya fue eliminada");
            }

            if (entidad.TieneImagenPersonalizada && !string.IsNullOrEmpty(entidad.RutaFoto) && File.Exists(entidad.RutaFoto))
            {
                try
                {
                    File.Delete(entidad.RutaFoto);
                }
                catch { }
            }

            _context.Personas.Remove(entidad);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al eliminar persona: {ex.Message}", ex);
        }
    }

    public void GuardarFotoPersona(Persona persona, byte[] imagenBytes)
    {
        if (imagenBytes == null || imagenBytes.Length == 0)
        {
            persona.RutaFoto = null;
            return;
        }

        try
        {
            ImagePathHelper.EnsureImageDirectoryExists();
            string imagePath = ImagePathHelper.GetImagePathForPersona(persona);

            File.WriteAllBytes(imagePath, imagenBytes);
            persona.RutaFoto = imagePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al guardar la imagen: {ex.Message}", ex);
        }
    }

    public byte[] CargarFotoPersona(Persona persona)
    {
        try
        {
            if (persona.TieneImagenPersonalizada)
            {
                var rutaReal = persona.GetType().GetField("_rutaFoto", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(persona) as string;
                    
                if (!string.IsNullOrEmpty(rutaReal) && File.Exists(rutaReal))
                {
                    return File.ReadAllBytes(rutaReal);
                }
            }
            
            var rutaFoto = persona.RutaFoto;
            if (!string.IsNullOrEmpty(rutaFoto) && File.Exists(rutaFoto))
            {
                return File.ReadAllBytes(rutaFoto);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar imagen: {ex.Message}");
            return null;
        }
    }

    public Persona ObtenerPersonaPorMatricula(string matricula)
    {
        if (string.IsNullOrWhiteSpace(matricula))
            return null;

        try
        {
            return _context.Personas
                .AsNoTracking()
                .FirstOrDefault(p => p.Matricula == matricula);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar persona por matrícula: {ex.Message}");
        }
    }

    public List<Persona> BuscarPersonas(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return new List<Persona>();

        try
        {
            termino = termino.ToLower();

            return _context.Personas
                .AsNoTracking()
                .Where(p =>
                    p.Nombre.ToLower().Contains(termino) ||
                    p.Apellido.ToLower().Contains(termino) ||
                    p.Matricula.ToLower().Contains(termino))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar personas: {ex.Message}");
        }
    }

    public (int Total, int ConFoto, int SinFoto) ObtenerEstadisticas()
    {
        try
        {
            var personas = _context.Personas.AsNoTracking().ToList();
            var total = personas.Count;
            var conFoto = personas.Count(p => p.TieneImagenPersonalizada);
            var sinFoto = total - conFoto;

            return (total, conFoto, sinFoto);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public List<string> ObtenerTiposPersona()
    {
        return _context.Personas
            .Select(p => p.TipoPersona)
            .Distinct()
            .Where(t => !string.IsNullOrEmpty(t))
            .OrderBy(t => t)
            .AsNoTracking()
            .ToList();
    }

    public Persona ObtenerPersonaPorId(int id)
    {
        return _context.Personas
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);
    }
}