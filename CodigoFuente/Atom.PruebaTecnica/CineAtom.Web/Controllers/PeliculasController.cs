using CineAtom.Web.DTOs.Pelicula;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CineAtom.Web.Data;
using CineAtom.Web.Models;
using CineAtom.Web.Helpers;

namespace CineAtom.Web.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly HttpClient _httpClient;
        // Clave API para autenticacion con OMDB
        private readonly string _omdbApiKey;
        // URL base para la API de OMDB
        private readonly string _omdbBaseUrl = "https://www.omdbapi.com/";

        public PeliculasController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _omdbApiKey = configuration["ApiSettings:ApiKey"];
        }

        #region Listado

        /// <summary>
        /// Muestra lista de peliculas con opciones de busqueda, paginacion y ordenamiento
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
           string search = "movie",
           string year = "",
           string type = "",
           string sortBy = "title",
           int page = 1,
           int pageSize = 10)
        {
            try
            {
                ValidarParametros(ref search, ref page, ref pageSize);

                var omdbUrl = ConstruirUrlBusqueda(search, year, type, page);
                var omdbResponse = await ObtenerRespuestaOmdb(omdbUrl);

                if (!RespuestaOmdbValida(omdbResponse))
                {
                    var mensajeError = "No se encontraron resultados para la búsqueda realizada.";
                    if (omdbResponse != null && !string.IsNullOrEmpty(omdbResponse.Error))
                    {
                        mensajeError = omdbResponse.Error;
                    }

                    NotificacionHelper.AgregarNotificacionError(this, mensajeError);
                    return RedirectToAction("Index");
                }

                var peliculas = await ObtenerDetallesPeliculas(omdbResponse.Search, pageSize);
                peliculas = OrdenarPeliculas(peliculas, sortBy);

                var paginacion = ConfigurarPaginacion(peliculas, omdbResponse.totalResults, page, pageSize);

                if (paginacion == null)
                {
                    return RedirectToAction("Index", new { search, year, type, sortBy, page, pageSize });
                }

                ConfigurarParametrosVista(search, year, type, sortBy, pageSize);

                string mensajeExito = $"Se encontraron {omdbResponse.totalResults} películas para la búsqueda '{search}'.";
                NotificacionHelper.AgregarNotificacionExito(this, mensajeExito);

                return View(paginacion);
            }
            catch (Exception ex)
            {
                return MostrarError("Ocurrió un error inesperado", ex.Message);
            }
        }

        #endregion

        #region Validaciones y Configuracion

        /// <summary>
        /// Valida y ajusta los parametros de entrada para la busqueda
        /// </summary>
        private void ValidarParametros(ref string search, ref int page, ref int pageSize)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                search = "movie";
            }

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : (pageSize > 100 ? 100 : pageSize);
        }

        /// <summary>
        /// Establece los parametros de busqueda en ViewBag para mantener el estado en la vista
        /// </summary>
        private void ConfigurarParametrosVista(string search, string year, string type, string sortBy, int pageSize)
        {
            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;
            ViewBag.Year = year;
            ViewBag.Type = type;
            ViewBag.SortBy = sortBy;
        }

        #endregion

        #region Hacer URLs y Peticiones HTTP

        /// <summary>
        /// Construye la URL para consultar la API de OMDB con los parametros dados
        /// </summary>
        private string ConstruirUrlBusqueda(string search, string year, string type, int page)
        {
            var omdbUrl = $"{_omdbBaseUrl}?apikey={_omdbApiKey}&s={Uri.EscapeDataString(search)}&page={page}";

            if (!string.IsNullOrEmpty(year))
            {
                omdbUrl += $"&y={year}";
            }

            if (!string.IsNullOrEmpty(type))
            {
                omdbUrl += $"&type={type}";
            }

            return omdbUrl;
        }

        /// <summary>
        /// Realiza la peticion a la API de OMDB y devuelve la respuesta deserializada
        /// </summary>
        private async Task<OMDbSearchResponse> ObtenerRespuestaOmdb(string omdbUrl)
        {
            var response = await _httpClient.GetAsync(omdbUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OMDbSearchResponse>(content);
        }

        #endregion

        #region Validaciones de respuesta

        /// <summary>
        /// Verifica si la respuesta de OMDB es valida y contiene resultados
        /// </summary>
        private bool RespuestaOmdbValida(OMDbSearchResponse omdbResponse)
        {
            // Verifica que la respuesta no sea nula, que el flag Response sea true, y que la lista Search tenga al menos un resultado
            return omdbResponse != null && omdbResponse.Response && omdbResponse.Search != null && omdbResponse.Search.Any();
        }


        #endregion

        #region Obtencion y Procesamiento de Datos de Peliculas

        /// <summary>
        /// Obtiene los detalles completos de cada pelicula en la lista de resultados
        /// </summary>
        private async Task<List<PeliculaDTO>> ObtenerDetallesPeliculas(List<OMDbMovie> resultados, int pageSize)
        {
            var peliculas = new List<PeliculaDTO>();

            foreach (var item in resultados.Take(pageSize))
            {
                var pelicula = await ObtenerDetallePelicula(item.imdbID);
                if (pelicula != null)
                {
                    peliculas.Add(pelicula);
                }
            }

            return peliculas;
        }

        /// <summary>
        /// Obtiene los detalles de una pelicula especifica por su ID de IMDB
        /// </summary>
        private async Task<PeliculaDTO> ObtenerDetallePelicula(string imdbId)
        {
            var response = await _httpClient.GetAsync($"{_omdbBaseUrl}?apikey={_omdbApiKey}&i={imdbId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var detail = JsonConvert.DeserializeObject<OMDbMovieDetail>(content);

            return MapearADto(detail);
        }

        /// <summary>
        /// Convierte los detalles de pelicula de OMDB a nuestro DTO
        /// </summary>
        private PeliculaDTO MapearADto(OMDbMovieDetail detail)
        {
            if (detail == null)
            {
                return null;
            }

            return new PeliculaDTO
            {
                ImdbID = detail.imdbID,
                Titulo = detail.Title,
                Anio = detail.Year,
                Poster = detail.Poster,
                Genero = detail.Genre,
                Director = detail.Director,
                Actores = detail.Actors,
                Duracion = detail.Runtime,
                ImdbRating = detail.imdbRating,
                Plot = detail.Plot
            };
        }

        #endregion

        #region Ordenamiento y Paginación

        /// <summary>
        /// Ordena la lista de peliculas segun el criterio especificado
        /// </summary>
        private List<PeliculaDTO> OrdenarPeliculas(List<PeliculaDTO> peliculas, string sortBy)
        {
            switch (sortBy.ToLower())
            {
                case "year":
                    return peliculas.OrderBy(p => p.Anio).ToList();
                case "year_desc":
                    return peliculas.OrderByDescending(p => p.Anio).ToList();
                case "title_desc":
                    return peliculas.OrderByDescending(p => p.Titulo).ToList();
                default:
                    return peliculas.OrderBy(p => p.Titulo).ToList();
            }
        }

        /// <summary>
        /// Configura el objeto de paginacion con los resultados y metadatos necesarios
        /// </summary>
        private Paginacion<PeliculaDTO> ConfigurarPaginacion(List<PeliculaDTO> peliculas, string totalResults, int page, int pageSize)
        {
            int total = 0;
            int.TryParse(totalResults, out total);

            int totalPages = (int)Math.Ceiling((double)total / pageSize);

            if (page > totalPages && totalPages > 0)
            {
                page = totalPages;
                return null;
            }

            return new Paginacion<PeliculaDTO>(peliculas, total, page, pageSize);
        }

        #endregion

        #region Manejo de Errores

        /// <summary>
        /// Muestra la vista de error con el mensaje correspondiente
        /// </summary>
        private IActionResult MostrarError(string error, string detalle = null)
        {
            ViewBag.Error = error;
            ViewBag.Detail = detalle;
            return View("Error");
        }

        #endregion
    }
}
