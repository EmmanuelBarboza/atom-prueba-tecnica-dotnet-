using CineAtom.WebApi.Data;
using CineAtom.WebApi.DTOs.Articulo;
using CineAtom.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

                if (dto.Cantidad < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener cantidad negativa."
                    });
                }

                if (dto.Precio < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener precio negativo."
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

        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateArticuloDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El objeto enviado es nulo."
                    });
                }

                if (dto.Cantidad < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener cantidad negativa."
                    });
                }

                if (dto.Precio < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener precio negativo."
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

                var connection = _context.Database.GetDbConnection();

                await connection.OpenAsync();

                var command = connection.CreateCommand();

                command.CommandText = "usp_CineAtom_Articulo_Update";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ArticuloId", SqlDbType.Int)
                {
                    Value = id
                });
                command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100)
                {
                    Value = dto.Nombre
                });
                command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 255)
                {
                    Value = dto.Descripcion
                });
                command.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int)
                {
                    Value = dto.Cantidad
                });
                command.Parameters.Add(new SqlParameter("@Precio", SqlDbType.Decimal)
                {
                    Value = dto.Precio
                });
                command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int)
                {
                    Value = dto.CategoriaId
                });

                var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParameter);

                await command.ExecuteNonQueryAsync();

                var result = (int) returnParameter.Value;

                if (result == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Artículo actualizado correctamente."
                    });
                }
                else if (result == -2)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No existe un artículo con ID {id}."
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error desconocido al actualizar el artículo."
                    });
                }
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede actualizar el artículo, error con la base de datos.",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al actualizar el artículo.",
                    detail = ex.Message
                });
            }
        }



        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();

                await connection.OpenAsync();

                var command = connection.CreateCommand();

                command.CommandText = "usp_CineAtom_Articulo_Delete";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ArticuloId", SqlDbType.Int) { Value = id });

                var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };

                command.Parameters.Add(returnParameter);

                await command.ExecuteNonQueryAsync();

                var result = (int)returnParameter.Value;

                if (result == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Artículo eliminado correctamente."
                    });
                }
                else if (result == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar un artículo con cantidad mayor a cero."
                    });
                }
                else if (result == -2)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No existe un artículo con ID {id}."
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error desconocido al eliminar el artículo."
                    });
                }
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede eliminar el artículo.",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al eliminar el artículo.",
                    detail = ex.Message
                });
            }
        }





    }
}
