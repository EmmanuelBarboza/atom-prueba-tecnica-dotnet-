using CineAtom.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CineAtom.Web.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public CategoriasController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]; // URL base de la API
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Categoria");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Error al cargar las categorías.";
                    return View(new List<Categoria>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var categorias = JsonConvert.DeserializeObject<List<Categoria>>(content);

                return View(categorias);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error inesperado: " + ex.Message;
                return View(new List<Categoria>());
            }
        }
    }
}
