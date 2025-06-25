using CineAtom.WebApi.DTOs.Pelicula;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CineAtom.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeliculasController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PeliculasController> _logger;

        private const string OMDbApiKey = "1c57ab32";
        private const string OMDbBaseUrl = "https://www.omdbapi.com/";

        public PeliculasController(HttpClient httpClient, ILogger<PeliculasController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("Buscar")]
        public async Task<IActionResult> BuscarPeliculas([FromQuery] string titulo, [FromQuery] int pagina = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe proporcionar un título para buscar películas."
                    });
                }

                string baseUrl = OMDbBaseUrl;
                string apiKey = OMDbApiKey;
                string encodedTitle = Uri.EscapeDataString(titulo);
                string queryString = $"apikey={apiKey}&s={encodedTitle}&page={pagina}";
                string fullUrl = $"{baseUrl}?{queryString}";

                HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = "Error al consultar la API externa de películas."
                    });
                }

                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<OMDbSearchResponseDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (resultado == null || resultado.Search == null || !resultado.Search.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No se encontraron resultados para la búsqueda."
                    });
                }


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
                _logger.LogError(ex, "Error inesperado al buscar películas.");

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
