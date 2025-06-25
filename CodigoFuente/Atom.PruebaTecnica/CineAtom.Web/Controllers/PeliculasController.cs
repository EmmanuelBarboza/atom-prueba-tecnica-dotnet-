using CineAtom.Web.DTOs.Pelicula;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CineAtom.Web.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public PeliculasController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:PeliculaURL"]; // Sacamos la url del appsettings.json
        }





        [HttpGet]
        public async Task<IActionResult> Index(string titulo = "Tom", int pagina = 1)
        {
            try
            {
                // Validacion de si el titulo es nulo o vacio
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    ViewBag.Message = "Ingrese un título para buscar películas."; //Con esto mostraremos mensaje en la vista
                    return View(new OMDbSearchResponseDTO());
                }

                var endpoint = $"{_apiBaseUrl}/api/Peliculas/Buscar?titulo={Uri.EscapeDataString(titulo)}&pagina={pagina}";

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Error al obtener las películas.";
                    return View(new OMDbSearchResponseDTO());
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OMDbSearchResponseDTO>(content);

                return View(result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error inesperado.";
                ViewBag.Detail = ex.Message;
                return View(new OMDbSearchResponseDTO());
            }
        }
    }
}
