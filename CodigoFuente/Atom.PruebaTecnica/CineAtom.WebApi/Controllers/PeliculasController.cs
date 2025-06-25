using CineAtom.WebApi.DTOs.Pelicula;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CineAtom.WebApi.Controllers
{
    /// <summary>
    /// controlador para manejar solicitudes relacionadas con peliculas
    /// usa una api externa para obtener informacion de peliculas segun titulo y pagina
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PeliculasController : ControllerBase
    {
        /// <summary>
        /// cliente http para hacer llamadas a la api externa
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// logger para registrar errores o informacion relevante
        /// </summary>
        private readonly ILogger<PeliculasController> _logger;

        /// <summary>
        /// clave api para autenticar peticiones a la api omdb
        /// </summary>
        private const string OMDbApiKey = "1c57ab32";

        /// <summary>
        /// url base de la api omdb
        /// </summary>
        private const string OMDbBaseUrl = "https://www.omdbapi.com/";

        /// <summary>
        /// constructor recibe cliente http y logger para usarlos en el controlador
        /// </summary>
        public PeliculasController(HttpClient httpClient, ILogger<PeliculasController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// metodo para buscar peliculas por titulo y pagina
        /// recibe parametros por query titulo obligatorio y pagina opcional que por defecto es 1
        /// devuelve resultados si los encuentra o errores segun sea el caso
        /// </summary>
        [HttpGet("Buscar")]
        public async Task<IActionResult> BuscarPeliculas([FromQuery] string titulo, [FromQuery] int pagina = 1)
        {
            try
            {
                // valida que titulo no sea vacio o nulo
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe proporcionar un título para buscar películas."
                    });
                }

                // prepara la url completa para llamar la api externa usando la clave la busqueda y pagina
                string baseUrl = OMDbBaseUrl;
                string apiKey = OMDbApiKey;
                string encodedTitle = Uri.EscapeDataString(titulo);
                string queryString = $"apikey={apiKey}&s={encodedTitle}&page={pagina}";
                string fullUrl = $"{baseUrl}?{queryString}";

                // hace la llamada http get a la api
                HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);

                // si la respuesta no es exitosa devuelve error con codigo http y mensaje
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = "Error al consultar la API externa de películas."
                    });
                }

                // lee el contenido json de la respuesta
                var json = await response.Content.ReadAsStringAsync();

                // deserializa el json a un objeto dto ignorando mayusculas o minusculas
                var resultado = JsonSerializer.Deserialize<OMDbSearchResponseDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // si no hay resultados encontrados devuelve not found con mensaje
                if (resultado == null || resultado.Search == null || !resultado.Search.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No se encontraron resultados para la búsqueda."
                    });
                }

                // si todo esta bien devuelve los resultados con exito
                return Ok(new
                {
                    success = true,
                    data = resultado.Search,
                    totalResults = resultado.TotalResults,
                    currentPage = pagina
                });
            }
            catch (Exception ex)
            {
                // registra el error inesperado en el log
                _logger.LogError(ex, "Error inesperado al buscar películas.");

                // devuelve error 500 con mensaje y detalle del error
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al buscar películas.",
                    detail = ex.Message
                });
            }
        }
    }
}
