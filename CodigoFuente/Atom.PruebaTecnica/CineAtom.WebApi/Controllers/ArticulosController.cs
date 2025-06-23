using CineAtom.WebApi.Data;
using CineAtom.WebApi.DTOs.Articulo;
using CineAtom.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CineAtom.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticulosController : Controller
    {
        private readonly CineAtomDbContext _context;

        public ArticulosController(CineAtomDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetArticulos()
        {
            try
            {
                var articulos = await _context.Articulos.Include(a => a.Categoria).ToListAsync();
                return Ok(new { success = true, data = articulos });
            }
            catch (SqlException sqlEx)
            {
                // Error relacionado con la base de datos (conexión, consulta, etc)
                return StatusCode(500, new { success = false, message = "Error en la base de datos al obtener artículos.", detail = sqlEx.Message });
            }
            catch (TimeoutException timeoutEx)
            {
                // Timeout en la operación
                return StatusCode(504, new { success = false, message = "Tiempo de espera agotado al obtener artículos.", detail = timeoutEx.Message });
            }
            catch (ArgumentNullException argNullEx)
            {
                // Argumento inválido o nulo
                return BadRequest(new { success = false, message = "Argumento inválido recibido.", detail = argNullEx.Message });
            }
            catch (Exception ex)
            {
                // Error general inesperado
                return StatusCode(500, new { success = false, message = "Error inesperado al obtener artículos.", detail = ex.Message });
            }
        }

        [HttpGet("Buscar/{id}")]
        public async Task<IActionResult> Buscar(int id)
        {
            try
            {
                var articulo = await _context.Articulos.Include(a => a.Categoria)
                    .FirstOrDefaultAsync(x => x.ArticuloId == id);

                if (articulo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró un artículo con ID {id}."
                    });
                }

                return Ok(new { success = true, data = articulo });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos al buscar el artículo.",
                    detail = sqlEx.Message
                });
            }
            catch (TimeoutException timeoutEx)
            {
                return StatusCode(504, new
                {
                    success = false,
                    message = "La solicitud de búsqueda del artículo excedió el tiempo de espera.",
                    detail = timeoutEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al buscar el artículo.",
                    detail = ex.Message
                });
            }
        }

        [HttpPost("Agregar")]
        public async Task<IActionResult> Agregar([FromBody] CreateArticuloDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo enviado es nulo."
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del artículo es obligatorio."
                    });
                }

                var categoriaExiste = await _context.Categorias.AnyAsync(c => c.CategoriaId == dto.CategoriaId);
                if (!categoriaExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"La categoría con ID {dto.CategoriaId} no existe."
                    });
                }

                var nuevoArticulo = new Articulo
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Cantidad = dto.Cantidad,
                    Precio = dto.Precio,
                    CategoriaId = dto.CategoriaId
                };

                await _context.Articulos.AddAsync(nuevoArticulo);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Buscar), new { id = nuevoArticulo.ArticuloId }, new
                {
                    success = true,
                    message = "Artículo agregado correctamente.",
                    data = nuevoArticulo
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al guardar el artículo en la base de datos.",
                    detail = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al agregar el artículo.",
                    detail = ex.Message
                });
            }
        }




    }
}
