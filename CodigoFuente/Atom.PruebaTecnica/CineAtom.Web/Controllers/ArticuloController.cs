using CineAtom.Web.DTOs.Articulo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using CineAtom.Web.Models;
using CineAtom.Web.Data;
using System.Net;
using CineAtom.Web.Helpers;


/// <summary>
/// Controlador encargado de gestionar las operaciones CRUD (crear, leer, actualizar, eliminar)
/// de artículos consumiendo servicios RESTful la API CineAtom.WebApi. Permite aplicar filtros
/// por nombre, categoría y rango de precios, así como paginar los resultados y mostrar
/// formularios modales para editar o eliminar artículos.
/// </summary>
namespace CineAtom.Web.Controllers
{
    /// <summary>
    /// Controlador para gestionar operaciones relacionadas con artículos
    /// </summary>
    public class ArticuloController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        #region Index y constructor
        /// <summary>
        /// Constructor del controlador Articulo
        /// </summary>
        /// <param name="httpClientFactory">Factory para crear clientes HTTP</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        public ArticuloController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:PeliculaURL"];
        }

        /// <summary>
        /// Muestra la lista de artículos con filtros y paginación
        /// </summary>
        /// <param name="filtroNombre">Filtro por nombre del artículo</param>
        /// <param name="filtroCategoriaId">Filtro por ID de categoría</param>
        /// <param name="precioMin">Precio mínimo para filtrar</param>
        /// <param name="precioMax">Precio máximo para filtrar</param>
        /// <param name="paginaInicio">Número de página inicial</param>
        /// <param name="cantidadregistros">Cantidad de registros por página</param>
        /// <returns>Vista con los artículos</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string filtroNombre, int? filtroCategoriaId, decimal? precioMin, decimal? precioMax, int paginaInicio = 1, int cantidadregistros = 10)
        {
            try
            {
                var articulos = await ObtenerArticulosDesdeAPI();
                if (articulos == null)
                {
                    return VistaConError("Error al cargar los artículos.");
                }

                var articulosFiltrados = FiltrarArticulos(articulos, filtroNombre, filtroCategoriaId, precioMin, precioMax);
                var paginacion = Paginacion<ArticuloDTO>.CrearPaginacion(articulosFiltrados, paginaInicio, cantidadregistros);

                await CargarCategoriasEnViewBag();
                ConfigurarViewBagFiltros(filtroNombre, filtroCategoriaId, precioMin, precioMax, cantidadregistros);

                return View(paginacion);
            }
            catch (Exception ex)
            {
                return VistaConError("Ocurrió un error inesperado.", ex.Message);
            }
        }

        #endregion

        #region Métodos privados para Index

        /// <summary>
        /// Obtiene la lista de artículos desde la API
        /// </summary>
        /// <returns>Lista de artículos o null si hay error</returns>
        private async Task<List<ArticuloDTO>> ObtenerArticulosDesdeAPI()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Articulos");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ArticuloListResponseDTO>(content)?.Data;
        }

        /// <summary>
        /// Filtra la lista de artículos según los parámetros
        /// </summary>
        /// <param name="articulos">Lista completa de artículos</param>
        /// <param name="filtroNombre">Filtro por nombre</param>
        /// <param name="filtroCategoriaId">Filtro por ID de categoría</param>
        /// <param name="precioMin">Filtro por precio mínimo</param>
        /// <param name="precioMax">Filtro por precio máximo</param>
        /// <returns>Lista de artículos filtrados</returns>
        private IEnumerable<ArticuloDTO> FiltrarArticulos(
            List<ArticuloDTO> articulos,
            string filtroNombre,
            int? filtroCategoriaId,
            decimal? precioMin,
            decimal? precioMax)
        {
            var filtrados = articulos.AsEnumerable();

            if (!string.IsNullOrEmpty(filtroNombre))
            {
                filtrados = filtrados.Where(a => a.Nombre.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase));
            }

            if (filtroCategoriaId.HasValue)
            {
                filtrados = filtrados.Where(a => a.Categoria?.CategoriaId == filtroCategoriaId.Value);
            }

            if (precioMin.HasValue)
            {
                filtrados = filtrados.Where(a => a.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                filtrados = filtrados.Where(a => a.Precio <= precioMax.Value);
            }

            return filtrados;
        }

        /// <summary>
        /// Carga las categorías desde la API y las guarda en ViewBag
        /// </summary>
        private async Task CargarCategoriasEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Categoria");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.Categorias = JsonConvert.DeserializeObject<List<Categoria>>(content);
                }
            }
            catch
            {
                ViewBag.Categorias = new List<Categoria>();
            }
        }

        /// <summary>
        /// Configura los filtros actuales en ViewBag
        /// </summary>
        /// <param name="filtroNombre">Filtro por nombre actual</param>
        /// <param name="filtroCategoriaId">Filtro por ID de categoría actual</param>
        /// <param name="precioMin">Filtro por precio mínimo actual</param>
        /// <param name="precioMax">Filtro por precio máximo actual</param>
        /// <param name="cantidadregistros">Cantidad de registros por página</param>
        private void ConfigurarViewBagFiltros(
            string filtroNombre,
            int? filtroCategoriaId,
            decimal? precioMin,
            decimal? precioMax,
            int cantidadregistros)
        {
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroCategoriaId = filtroCategoriaId;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.CantidadRegistros = cantidadregistros;
        }

        /// <summary>
        /// Retorna una vista con mensaje de error usando el helper de notificaciones
        /// </summary>
        /// <param name="error">Mensaje de error principal</param>
        /// <param name="detalle">Detalle del error (opcional)</param>
        /// <returns>Vista Index con mensaje de error</returns>
        private IActionResult VistaConError(string error, string detalle = null)
        {
            NotificacionHelper.AgregarNotificacionError(this, error, detalle);
            return View("Index", new Paginacion<ArticuloDTO>(new List<ArticuloDTO>(), 0, 1, ViewBag.CantidadRegistros ?? 10));
        }

        #endregion

        #region Metodo de agregar
        /// <summary>
        /// Agrega un nuevo artículo
        /// </summary>
        /// <param name="dto">DTO con los datos del artículo a crear</param>
        /// <returns>Redirección a Index o vista con error</returns>
        [HttpPost]
        public async Task<IActionResult> Agregar(CreateArticuloDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategorias();
                NotificacionHelper.AgregarNotificacionError(this, "Por favor corrija los errores del formulario.");
                return RedirectToAction("Index");
            }

            try
            {
                var response = await EnviarArticuloAAPI(dto, $"{_apiBaseUrl}/api/Articulos/Agregar", HttpMethod.Post);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    NotificacionHelper.AgregarNotificacionError(this, "No se pudo agregar el artículo.", errorContent);
                    return RedirectToAction("Index");
                }

                NotificacionHelper.AgregarNotificacionExito(this, "Artículo agregado correctamente.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                NotificacionHelper.AgregarNotificacionError(this, "Ocurrió un error inesperado.", ex.Message);
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Metodo editar
        /// <summary>
        /// Edita un artículo existente
        /// </summary>
        /// <param name="dto">DTO con los datos actualizados del artículo</param>
        /// <returns>Redirección a Index o vista con error</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateArticuloDTO dto)
        {
            if (!ModelState.IsValid)
            {
                NotificacionHelper.AgregarNotificacionError(this, "Por favor corrija los errores del formulario.");
                return await VistaEdicionConError(dto);
            }

            try
            {
                var response = await EnviarArticuloAAPI(dto, $"{_apiBaseUrl}/api/Articulos/Actualizar/{dto.ArticuloId}", HttpMethod.Put);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    NotificacionHelper.AgregarNotificacionError(this, "No se pudo actualizar el artículo.", errorContent);
                    return await VistaEdicionConError(dto);
                }

                NotificacionHelper.AgregarNotificacionExito(this, "Artículo actualizado correctamente.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                NotificacionHelper.AgregarNotificacionError(this, "Ocurrió un error inesperado.", ex.Message);
                return await VistaEdicionConError(dto);
            }
        }

        /// <summary>
        /// Muestra modal para editar artículo
        /// </summary>
        /// <param name="id">ID del artículo a editar</param>
        /// <returns>Vista Index con modal de edición</returns>
        [HttpGet]
        public async Task<IActionResult> EditarModal(int id)
        {
            try
            {
                var articulos = await ObtenerArticulosDesdeAPI();
                if (articulos == null)
                {
                    return VistaConError("Error al cargar los artículos.");
                }

                var articulo = articulos.FirstOrDefault(a => a.ArticuloId == id);
                if (articulo == null)
                {
                    return RedirectToAction("Index");
                }

                var paginacion = Paginacion<ArticuloDTO>.CrearPaginacion(articulos, 1, ViewBag.CantidadRegistros ?? 10);
                await ConfigurarVistaParaModalEditar(articulo);

                return View("Index", paginacion);
            }
            catch (Exception ex)
            {
                return VistaConError("Ocurrió un error inesperado.", ex.Message);
            }
        }


        #endregion

        #region Métodos privados para Editar

        /// <summary>
        /// Configura la vista para mostrar modal de edición
        /// </summary>
        /// <param name="articulo">DTO del artículo a editar</param>
        private async Task ConfigurarVistaParaModalEditar(ArticuloDTO articulo)
        {
            await CargarCategorias();
            ViewBag.ArticuloEditar = articulo;
            ViewBag.MostrarModalEditar = true;
        }

        #endregion

        #region Métodos privados para CRUD

        /// <summary>
        /// Envía datos de artículo a la API
        /// </summary>
        /// <param name="dto">DTO con los datos del artículo</param>
        /// <param name="url">URL del endpoint de la API</param>
        /// <param name="method">Método HTTP (POST o PUT)</param>
        /// <returns>Respuesta HTTP</returns>
        private async Task<HttpResponseMessage> EnviarArticuloAAPI(object dto, string url, HttpMethod method)
        {
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(dto),
                System.Text.Encoding.UTF8,
                "application/json");

            return method == HttpMethod.Put ?
                await _httpClient.PutAsync(url, jsonContent) :
                await _httpClient.PostAsync(url, jsonContent);
        }

        /// <summary>
        /// Prepara la vista de edición con mensaje de error
        /// </summary>
        /// <param name="dto">DTO con los datos del artículo</param>
        /// <param name="error">Mensaje de error</param>
        /// <param name="detalle">Detalle del error</param>
        /// <returns>Vista Index con datos de error</returns>
        private async Task<IActionResult> VistaEdicionConError(
            UpdateArticuloDTO dto,
            string error = null,
            string detalle = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                NotificacionHelper.AgregarNotificacionError(this, error, detalle);
            }

            var articulos = await ObtenerArticulosDesdeAPI();
            if (articulos == null)
            {
                return VistaConError("Error al cargar los artículos.");
            }

            var paginacion = Paginacion<ArticuloDTO>.CrearPaginacion(articulos, 1, ViewBag.CantidadRegistros ?? 10);
            await CargarCategorias();
            ViewBag.ArticuloEditar = dto;

            return View("Index", paginacion);
        }

        /// <summary>
        /// Maneja errores de la API
        /// </summary>
        /// <param name="response">Respuesta HTTP con error</param>
        /// <param name="mensaje">Mensaje de error</param>
        private async Task ManejarErrorDeAPI(HttpResponseMessage response, string mensaje)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            NotificacionHelper.AgregarNotificacionError(this, mensaje, errorContent);
            await CargarCategorias();
        }

        /// <summary>
        /// Maneja excepciones no controladas
        /// </summary>
        /// <param name="ex">Excepción capturada</param>
        private async Task ManejarErrorExcepcion(Exception ex)
        {
            NotificacionHelper.AgregarNotificacionError(this, "Ocurrió un error inesperado.", ex.Message);
            await CargarCategorias();
        }

        #endregion

        #region Metodo eliminar
        /// <summary>
        /// Muestra modal de confirmación para eliminar artículo
        /// </summary>
        /// <param name="id">ID del artículo a eliminar</param>
        /// <returns>Vista Index con modal de confirmación</returns>
        [HttpGet]
        public async Task<IActionResult> EliminarModal(int id)
        {
            try
            {
                var articulos = await ObtenerArticulosDesdeAPI();
                if (articulos == null)
                {
                    return VistaConError("Error al cargar los artículos.");
                }

                var articulo = articulos.FirstOrDefault(a => a.ArticuloId == id);
                if (articulo == null)
                {
                    return RedirectToAction("Index");
                }

                var paginacion = Paginacion<ArticuloDTO>.CrearPaginacion(articulos, 1, ViewBag.CantidadRegistros ?? 10);
                await ConfigurarVistaParaModalEliminar(articulo);

                return View("Index", paginacion);
            }
            catch (Exception ex)
            {
                return VistaConError("Ocurrió un error inesperado.", ex.Message);
            }
        }

        /// <summary>
        /// Elimina un artículo
        /// </summary>
        /// <param name="articuloId">ID del artículo a eliminar</param>
        /// <returns>Redirección a Index o vista con error</returns>
        [HttpPost]
        public async Task<IActionResult> Eliminar(int articuloId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Articulos/Eliminar/{articuloId}");

                if (!response.IsSuccessStatusCode)
                {
                    return await VistaEliminacionConError(articuloId,
                        "No se pudo eliminar el artículo.",
                        await response.Content.ReadAsStringAsync());
                }

                NotificacionHelper.AgregarNotificacionExito(this, "Artículo eliminado correctamente.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return await VistaEliminacionConError(articuloId,
                    "Ocurrió un error inesperado.",
                    ex.Message);
            }
        }
        #endregion

        #region Métodos privados para Eliminar

        /// <summary>
        /// Prepara vista de eliminación con mensaje de error
        /// </summary>
        /// <param name="articuloId">ID del artículo</param>
        /// <param name="error">Mensaje de error</param>
        /// <param name="detalle">Detalle del error</param>
        /// <returns>Vista Index con datos de error</returns>
        private async Task<IActionResult> VistaEliminacionConError(
            int articuloId,
            string error,
            string detalle)
        {
            NotificacionHelper.AgregarNotificacionError(this, error, detalle);

            var articulos = await ObtenerArticulosDesdeAPI();
            if (articulos == null)
            {
                return VistaConError("Error al cargar los artículos.");
            }

            var paginacion = Paginacion<ArticuloDTO>.CrearPaginacion(articulos, 1, ViewBag.CantidadRegistros ?? 10);
            await CargarCategorias();
            ViewBag.ArticuloEliminar = articulos.FirstOrDefault(a => a.ArticuloId == articuloId);

            return View("Index", paginacion);
        }

        /// <summary>
        /// Configura la vista para mostrar modal de eliminación
        /// </summary>
        /// <param name="articulo">DTO del artículo a eliminar</param>
        private async Task ConfigurarVistaParaModalEliminar(ArticuloDTO articulo)
        {
            await CargarCategorias();
            ViewBag.ArticuloEliminar = articulo;
            ViewBag.MostrarModalEliminar = true;
        }

        #endregion

        #region Misc
        /// <summary>
        /// Carga las categorías desde la API
        /// </summary>
        private async Task CargarCategorias()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Categoria");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.Categorias = JsonConvert.DeserializeObject<List<Categoria>>(content);
                }
            }
            catch
            {
                ViewBag.Categorias = new List<Categoria>();
            }
        }
        #endregion
    }
}