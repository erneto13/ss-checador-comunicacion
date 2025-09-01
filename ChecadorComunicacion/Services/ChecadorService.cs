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

    public string DeterminarTipoAccion(int personaId)
    {
        var ultimoChecador = ObtenerUltimoChecadorPorPersona(personaId);
        
        if (ultimoChecador == null || ultimoChecador.TipoAccion == "Salida")
        {
            return "Entrada";
        }
        
        return "Salida";
    }

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
        
        return _context.Checadores
            .Include(c => c.Persona)
            .FirstOrDefault(c => c.Id == nuevoChecador.Id);
    }
    
    public List<Checador> ObtenerChecadoresPorRangoFecha(DateOnly fechaInicio, DateOnly fechaFin)
    {
        return _context.Checadores
            .Include(c => c.Persona)
            .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Persona.Nombre)
            .ThenBy(c => c.Hora)
            .AsNoTracking()
            .ToList();
    }

    public List<Checador> ObtenerChecadoresPorRangoFechaYTipo(DateOnly fechaInicio, DateOnly fechaFin, string tipoPersona)
    {
        var query = _context.Checadores
            .Include(c => c.Persona)
            .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin);
    
        if (!string.IsNullOrEmpty(tipoPersona) && tipoPersona != "Todos")
        {
            query = query.Where(c => c.Persona.TipoPersona == tipoPersona);
        }
    
        return query
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Persona.Nombre)
            .ThenBy(c => c.Hora)
            .AsNoTracking()
            .ToList();
    }
}