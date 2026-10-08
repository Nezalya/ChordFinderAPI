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

        // Mapas de funciones armónicas según el grado
        string[] funcionesMayor = { "Tónica", "Supertonica", "Mediante", "Subdominante", "Dominante", "Submediante", "Sensible" };
        string[] funcionesMenor = { "Tónica", "Supertonica", "Mediante", "Subdominante", "Dominante", "Submediante", "Sensible" };
        string[] romanosMayor = { "I", "ii", "iii", "IV", "V", "vi", "vii°" };
        string[] romanosMenor = { "i", "ii°", "III", "iv", "v", "VI", "VII" };

        var resultado = new
        {
            AcordeBuscado = acordeBuscado.Nombre,
            NotasDelAcorde = acordeBuscado.Notas,
            TotalEscalasEncontradas = escalas.Count,
            EscalasDondeAparece = escalas.Select(e =>
            {
                var notasEscala = e.Notas.Split(',').Select(n => n.Trim()).ToList();
                bool esMayor = e.Tipo.Equals("Mayor", StringComparison.OrdinalIgnoreCase);

                // Determinar Relativa
                string relativaNombre = "";
                string relativaTipo = esMayor ? "Menor" : "Mayor";
                if (esMayor)
                {
                    // La relativa menor está en el grado 6 (índice 5)
                    relativaNombre = $"{notasEscala[5]} Menor";
                }
                else
                {
                    // La relativa mayor está en el grado 3 (índice 2)
                    relativaNombre = $"{notasEscala[2]} Mayor";
                }

                // Construir detalle de los 7 acordes con sus notas y función
                var acordesDetallados = e.Acordes
                    .OrderBy(a =>
                    {
                        string notaTonica = a.Nombre.EndsWith("dim") ? a.Nombre.Replace("dim", "")
                            : a.Nombre.EndsWith("m") && !a.Nombre.EndsWith("maj7") ? a.Nombre[..^1]
                            : a.Nombre.Replace("maj7", "").Replace("7", "");
                        int pos = notasEscala.IndexOf(notaTonica);
                        return pos >= 0 ? pos : 99;
                    })
                    .Select((a, idx) => new
                    {
                        Grado = esMayor ? romanosMayor[idx] : romanosMenor[idx],
                        Acorde = a.Nombre,
                        Notas = a.Notas,
                        Funcion = esMayor ? funcionesMayor[idx] : funcionesMenor[idx]
                    })
                    .ToList();

                // Grado del acorde buscado en esta escala
                var infoAcordeBuscado = acordesDetallados.FirstOrDefault(a => a.Acorde.Equals(acordeBuscado.Nombre, StringComparison.OrdinalIgnoreCase));

                return new
                {
                    NombreEscala = e.Nombre,
                    TipoEscala = e.Tipo,
                    NotasDeLaEscala = e.Notas,
                    Relativa = new { Nombre = relativaNombre, Tipo = relativaTipo },
                    GradoAcordeBuscado = infoAcordeBuscado?.Grado ?? "I",
                    FuncionAcordeBuscado = infoAcordeBuscado?.Funcion ?? "Tónica",
                    AcordesDeLaEscala = acordesDetallados
                };
            })
        };

        return Ok(resultado);
    }
}