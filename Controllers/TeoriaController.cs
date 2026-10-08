using ChordFinderAPI.Helpers;
using ChordFinderAPI.Models;
using ChordFinderAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChordFinderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeoriaController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeoriaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("cargar-teoria-basica")]
    public async Task<IActionResult> CargarTeoria()
    {
        if (await _context.Acordes.AnyAsync() || await _context.Escalas.AnyAsync())
            return BadRequest("La base de datos ya contiene información musical.");

        // Generar todos los acordes automáticamente
        var diccionarioAcordes = GeneradorDeAcordes.GenerarAcordes();

        // Generar las escalas y vincularles sus acordes armónicos
        var listaEscalas = GeneradorDeAcordes.GenerarEscalasConArmonia(diccionarioAcordes);

        // Guardar en EF Core
        await _context.Acordes.AddRangeAsync(diccionarioAcordes.Values);
        await _context.Escalas.AddRangeAsync(listaEscalas);
        await _context.SaveChangesAsync();

        return Ok($"¡Teoría cargada con éxito! Se registraron {diccionarioAcordes.Count} acordes y {listaEscalas.Count} escalas con sus relaciones armónicas.");
    }

    [HttpGet("buscar-escalas-por-acorde/{nombreAcorde}")]
    public async Task<IActionResult> BuscarEscalasPorAcorde(string nombreAcorde)
    {
        var nombreDecodificado = Uri.UnescapeDataString(nombreAcorde).Trim();

        var acordeBuscado = await _context.Acordes
            .FirstOrDefaultAsync(a => a.Nombre.ToLower() == nombreDecodificado.ToLower());

        if (acordeBuscado == null)
        {
            return NotFound($"No se encontró el acorde '{nombreDecodificado}'.");
        }

        var escalas = await _context.Escalas
            .Include(e => e.Acordes)
            .Where(e => e.Acordes.Any(a => a.Id == acordeBuscado.Id))
            .ToListAsync();

        var resultado = new
        {
            AcordeBuscado = acordeBuscado.Nombre,
            NotasDelAcorde = acordeBuscado.Notas,
            TotalEscalasEncontradas = escalas.Count,
            EscalasDondeAparece = escalas.Select(e =>
            {
                var listaNotasEscala = e.Notas
                    .Split(',')
                    .Select(n => n.Trim())
                    .ToList();

                var acordesOrdenados = e.Acordes
                    .OrderBy(acorde =>
                    {
                        string notaTonicaAcorde = acorde.Nombre.EndsWith("dim")
                            ? acorde.Nombre.Replace("dim", "")
                            : acorde.Nombre.EndsWith("m") && !acorde.Nombre.EndsWith("maj7")
                                ? acorde.Nombre.Substring(0, acorde.Nombre.Length - 1)
                                : acorde.Nombre.Replace("maj7", "").Replace("7", "");

                        int posicion = listaNotasEscala.IndexOf(notaTonicaAcorde);
                        return posicion >= 0 ? posicion : 99; // Retorna el grado (0 a 6 / I a VII)
                    })
                    .Select(a => a.Nombre)
                    .ToList();

                return new
                {
                    NombreEscala = e.Nombre,
                    TipoEscala = e.Tipo,
                    NotasDeLaEscala = e.Notas,
                    AcordesDeLaEscala = acordesOrdenados
                };
            })
        };

        return Ok(resultado);
    }
}