using System;
using System.Collections.Generic;
using System.Linq;
using ChecadorComunicacion.Data;
using Microsoft.EntityFrameworkCore;

namespace ChecadorComunicacion.Services;

public class ChecadorService
{
    private readonly AppDbContext _context = new AppDbContext();

    public List<Checador> ObtenerChecadores()
    {
        return _context.Checadores
            .Include(c => c.Persona)
            .AsNoTracking()
            .ToList();
    }

    public List<Checador> ObtenerChecadoresPorPersona(int personaId)
    {
        return _context.Checadores
            .Include(c => c.Persona)
            .Where(c => c.PersonaId == personaId)
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Hora)
            .AsNoTracking()
            .ToList();
    }

    public Checador ObtenerUltimoChecadorPorPersona(int personaId)
    {
        return _context.Checadores
            .Include(c => c.Persona)
            .Where(c => c.PersonaId == personaId)
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.Hora)
            .AsNoTracking()
            .FirstOrDefault();
    }

    public void AgregarChecador(Checador checador)
    {
        _context.Checadores.Add(checador);
        _context.SaveChanges();
    }

    public void ActualizarChecador(Checador checador)
    {
        using var context = new AppDbContext();
        context.Attach(checador);
        context.Entry(checador).State = EntityState.Modified;
        context.SaveChanges();
    }

    public void EliminarChecador(Checador checador)
    {
        var entidad = _context.Checadores.FirstOrDefault(c => c.Id == checador.Id)
                      ?? _context.Checadores.Find(checador.Id);

        if (entidad == null) return;
        _context.Checadores.Remove(entidad);
        _context.SaveChanges();
    }

    /// <summary>
    /// Determina si el próximo registro debe ser "Entrada" o "Salida" basado en el último registro de la persona
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>"Entrada" si el último fue "Salida" o no hay registros, "Salida" si el último fue "Entrada"</returns>
    public string DeterminarTipoAccion(int personaId)
    {
        var ultimoChecador = ObtenerUltimoChecadorPorPersona(personaId);
        
        // Si no hay registros previos o el último fue "Salida", el siguiente debe ser "Entrada"
        if (ultimoChecador == null || ultimoChecador.TipoAccion == "Salida")
        {
            return "Entrada";
        }
        
        // Si el último fue "Entrada", el siguiente debe ser "Salida"
        return "Salida";
    }

    /// <summary>
    /// Registra un nuevo checador para una persona, determinando automáticamente el tipo de acción
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>El checador creado</returns>
    public Checador RegistrarChecador(int personaId)
    {
        var tipoAccion = DeterminarTipoAccion(personaId);
        var ahora = DateTime.Now;
        
        var nuevoChecador = new Checador
        {
            PersonaId = personaId,
            TipoAccion = tipoAccion,
            Fecha = DateOnly.FromDateTime(ahora),
            Hora = TimeOnly.FromDateTime(ahora)
        };

        AgregarChecador(nuevoChecador);
        
        // Retornar el checador con la información de la persona cargada
        return _context.Checadores
            .Include(c => c.Persona)
            .FirstOrDefault(c => c.Id == nuevoChecador.Id);
    }
}