using CineAtom.Web.DTOs.Articulo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using CineAtom.Web.Models;

namespace CineAtom.Web.Controllers
{
    public class ArticuloController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ArticuloController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:PeliculaURL"];
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var articulosResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Articulos");
                if (!articulosResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Error al cargar los artículos.";
                    return View(new List<ArticuloDTO>());
                }

                var articulosContent = await articulosResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ArticuloListResponseDTO>(articulosContent);

                // Cargar categorías para el select del modal
                var categoriasResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Categoria");
                if (categoriasResponse.IsSuccessStatusCode)
                {
                    var categoriasContent = await categoriasResponse.Content.ReadAsStringAsync();
                    var categorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasContent);
                    ViewBag.Categorias = categorias;
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error inesperado.";
                ViewBag.Detail = ex.Message;
                return View(new List<ArticuloDTO>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(CreateArticuloDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategorias();
                return RedirectToAction("Index");
            }

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Articulos/Agregar", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "No se pudo agregar el artículo.";
                    ViewBag.Detail = await response.Content.ReadAsStringAsync();
                    await CargarCategorias();
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error inesperado.";
                ViewBag.Detail = ex.Message;
                await CargarCategorias();
                return RedirectToAction("Index");
            }
        }

        private async Task CargarCategorias()
        {
            try
            {
                var categoriasResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Categoria");
                if (categoriasResponse.IsSuccessStatusCode)
                {
                    var categoriasContent = await categoriasResponse.Content.ReadAsStringAsync();
                    var categorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasContent);
                    ViewBag.Categorias = categorias;
                }
            }
            catch
            {
                ViewBag.Categorias = new List<Categoria>();
            }
        }
    }

}
