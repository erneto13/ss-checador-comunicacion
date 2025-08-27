using System.Collections.Generic;
using System.Linq;
using ChecadorComunicacion.Data;
using ChecadorComunicacion.Models;
using Microsoft.EntityFrameworkCore;

namespace ChecadorComunicacion.Services;

public class PersonaService
{
    private readonly AppDbContext _context = new();

    public List<Persona> ObtenerPersonas()
    {
        return _context.Personas.AsNoTracking().ToList();
    }

    public void AgregarPersona(Persona persona)
    {
        _context.Personas.Add(persona);
        _context.SaveChanges();
    }

    public void ActualizarPersona(Persona persona)
    {
        using var context = new AppDbContext();
        context.Attach(persona);
        context.Entry(persona).State = EntityState.Modified;
        context.SaveChanges();
    }

    public void EliminarPersona(Persona persona)
    {
        var entidad = _context.Personas.FirstOrDefault(p => p.Id == persona.Id)
                      ?? _context.Personas.Find(persona.Id);

        if (entidad == null) return;
        _context.Personas.Remove(entidad);
        _context.SaveChanges();
    }
}