using CineAtom.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineAtom.WebApi.Controllers
{
    /// <summary>
    /// controlador para manejar peticiones relacionadas con categorias
    /// permite obtener la lista completa de categorias desde la base de datos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : Controller
    {
        /// <summary>
        /// contexto de base de datos para acceder a tablas y datos
        /// </summary>
        private readonly CineAtomDbContext _context;

        /// <summary>
        /// constructor recibe contexto de base de datos para usarlo
        /// </summary>
        public CategoriaController(CineAtomDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// metodo para obtener todas las categorias disponibles en la bd
        /// intenta obtener la lista y si hay error devuelve codigo 500 con mensaje
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                // obtiene la lista completa de categorias usando entity framework
                var categorias = await _context.Categorias.ToListAsync();
                // devuelve ok con la lista de categorias
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                // en caso de error devuelve codigo 500 y mensaje con detalle del error
                return StatusCode(500, $"Error al obtener las categorias: {ex.Message}");
            }
        }
    }
}
