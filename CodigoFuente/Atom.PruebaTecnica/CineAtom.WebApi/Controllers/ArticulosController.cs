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
        #region Constructor y Dependencies
        private readonly CineAtomDbContext _context;

        public ArticulosController(CineAtomDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Operaciones GET
        /// <summary>
        /// Obtiene todos los articulos con sus categorias
        /// <returns>Lista de articulos con informacion de categoria</returns>
        [HttpGet]
        public async Task<IActionResult> GetArticulos()
        {
            try
            {
                // Obtener todos los articulos incluyendo la informacion de categoria
                var articulos = await _context.Articulos.Include(a => a.Categoria).ToListAsync();

                // Retornar lista de articulos con formato de exito
                return Ok(new { success = true, data = articulos });
            }
            catch (SqlException sqlEx)
            {
                // Error relacionado con la base de datos 
                return StatusCode(500, new { success = false, message = "Error en la base de datos al obtener artículos.", detail = sqlEx.Message });
            }
            catch (TimeoutException timeoutEx)
            {
                // Timeout en la operacion
                return StatusCode(504, new { success = false, message = "Tiempo de espera agotado al obtener artículos.", detail = timeoutEx.Message });
            }
            catch (ArgumentNullException argNullEx)
            {
                // Argumento no valido o nulo
                return BadRequest(new { success = false, message = "Argumento inválido recibido.", detail = argNullEx.Message });
            }
            catch (Exception ex)
            {
                // Error general 
                return StatusCode(500, new { success = false, message = "Error inesperado al obtener artículos.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Busca un articulo por su ID
        /// <param name="id">ID del articulo a buscar</param>
        /// <returns>Articulo encontrado o mensaje de error</returns>
        [HttpGet("Buscar/{id}")]
        public async Task<IActionResult> Buscar(int id)
        {
            try
            {
                // Buscar articulo por ID incluyendo la categoria relacionada
                var articulo = await _context.Articulos.Include(a => a.Categoria)
                    .FirstOrDefaultAsync(x => x.ArticuloId == id);

                // Validar si el articulo existe
                if (articulo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró un artículo con ID {id}."
                    });
                }

                // Retornar articulo encontrado
                return Ok(new { success = true, data = articulo });
            }
            catch (SqlException sqlEx)
            {
                // Error especifico de SQL
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos al buscar el artículo.",
                    detail = sqlEx.Message
                });
            }
            catch (TimeoutException timeoutEx)
            {
                // Error por tiempo de espera agotado
                return StatusCode(504, new
                {
                    success = false,
                    message = "La solicitud de búsqueda del artículo excedió el tiempo de espera.",
                    detail = timeoutEx.Message
                });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al buscar el artículo.",
                    detail = ex.Message
                });
            }
        }
        #endregion

        #region Operaciones POST
        /// <summary>
        /// Agrega un nuevo articulo al sistema
        /// <param name="dto">DTO con los datos del nuevo articulo</param>
        /// <returns>Respuesta con el articulo creado o mensaje de error</returns>
        [HttpPost("Agregar")]
        public async Task<IActionResult> Agregar([FromBody] CreateArticuloDTO dto)
        {
            try
            {
                // Validar objeto recibido
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo enviado es nulo."
                    });
                }

                // Validar cantidad no negativa
                if (dto.Cantidad <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener cantidad negativa ni 0."
                    });
                }

                // Validar precio no negativo
                if (dto.Precio < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener precio negativo."
                    });
                }

                // Validar nombre obligatorio
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del artículo es obligatorio."
                    });
                }

                // Verificar existencia de la categoria
                var categoriaExiste = await _context.Categorias.AnyAsync(c => c.CategoriaId == dto.CategoriaId);
                if (!categoriaExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"La categoría con ID {dto.CategoriaId} no existe."
                    });
                }

                // Crear nuevo objeto Articulo
                var nuevoArticulo = new Articulo
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Cantidad = dto.Cantidad,
                    Precio = dto.Precio,
                    CategoriaId = dto.CategoriaId
                };

                // Agregar y guardar cambios
                await _context.Articulos.AddAsync(nuevoArticulo);
                await _context.SaveChangesAsync();

                // Retornar respuesta de creado con ubicacion del nuevo recurso
                return CreatedAtAction(nameof(Buscar), new { id = nuevoArticulo.ArticuloId }, new
                {
                    success = true,
                    message = "Artículo agregado correctamente.",
                    data = nuevoArticulo
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Error al actualizar la base de datos
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al guardar el artículo en la base de datos.",
                    detail = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al agregar el artículo.",
                    detail = ex.Message
                });
            }
        }
        #endregion

        #region Operaciones PUT
        /// <summary>
        /// Actualiza un articulo existente usando un stored procedure
        /// <param name="id">ID del articulo a actualizar</param>
        /// <param name="dto">DTO con los nuevos datos del articulo</param>
        /// <returns>Respuesta de exito o mensaje de error</returns>
        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateArticuloDTO dto)
        {
            try
            {
                // Validar objeto recibido
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El objeto enviado es nulo."
                    });
                }

                // Validar cantidad no negativa
                if (dto.Cantidad < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener cantidad negativa."
                    });
                }

                // Validar precio no negativo
                if (dto.Precio < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El artículo no puede tener precio negativo."
                    });
                }

                // Validar nombre obligatorio
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del artículo es obligatorio."
                    });
                }

                // Configurar conexion y comando para el stored procedure
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                var command = connection.CreateCommand();

                // Establecer parametros del stored procedure
                command.CommandText = "usp_CineAtom_Articulo_Update";
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parametros al comando
                command.Parameters.Add(new SqlParameter("@ArticuloId", SqlDbType.Int) { Value = id });
                command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = dto.Nombre });
                command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 255) { Value = dto.Descripcion });
                command.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = dto.Cantidad });
                command.Parameters.Add(new SqlParameter("@Precio", SqlDbType.Decimal) { Value = dto.Precio });
                command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = dto.CategoriaId });

                // Configurar parametro de retorno
                var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParameter);

                // Ejecutar stored procedure
                await command.ExecuteNonQueryAsync();

                // Evaluar resultado del stored procedure
                var result = (int)returnParameter.Value;

                if (result == 0)
                {
                    // Exito en la actualizacion
                    return Ok(new
                    {
                        success = true,
                        message = "Artículo actualizado correctamente."
                    });
                }
                else if (result == -2)
                {
                    // Articulo no encontrado
                    return NotFound(new
                    {
                        success = false,
                        message = $"No existe un artículo con ID {id}."
                    });
                }
                else
                {
                    // Error desconocido
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error desconocido al actualizar el artículo."
                    });
                }
            }
            catch (SqlException ex)
            {
                // Error especifico de SQL
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede actualizar el artículo, error con la base de datos.",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al actualizar el artículo.",
                    detail = ex.Message
                });
            }
        }
        #endregion

        #region Operaciones DELETE
        /// <summary>
        /// Elimina un articulo usando un stored procedure
        /// <param name="id">ID del articulo a eliminar</param>
        /// <returns>Respuesta de exito o mensaje de error</returns>
        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                // Configurar conexion y comando para el stored procedure
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                var command = connection.CreateCommand();

                // Establecer stored procedure de eliminacion
                command.CommandText = "usp_CineAtom_Articulo_Delete";
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parametro de ID
                command.Parameters.Add(new SqlParameter("@ArticuloId", SqlDbType.Int) { Value = id });

                // Configurar parametro de retorno
                var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParameter);

                // Ejecutar stored procedure
                await command.ExecuteNonQueryAsync();

                // Evaluar resultado del stored procedure
                var result = (int)returnParameter.Value;

                if (result == 0)
                {
                    // Exito en la eliminacion
                    return Ok(new
                    {
                        success = true,
                        message = "Artículo eliminado correctamente."
                    });
                }
                else if (result == -1)
                {
                    // Error: articulo con cantidad mayor a cero
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar un artículo con cantidad mayor a cero."
                    });
                }
                else if (result == -2)
                {
                    // Articulo no encontrado
                    return NotFound(new
                    {
                        success = false,
                        message = $"No existe un artículo con ID {id}."
                    });
                }
                else
                {
                    // Error desconocido
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error desconocido al eliminar el artículo."
                    });
                }
            }
            catch (SqlException ex)
            {
                // Error especifico de SQL
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede eliminar el artículo.",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Error inesperado
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al eliminar el artículo.",
                    detail = ex.Message
                });
            }
        }
        #endregion
    }
}