using CineAtom.Web.Controllers;

namespace CineAtom.Web.Models
{
    public class OMDbSearchResponse
    {
        /// <summary>
        /// lista de peliculas encontradas segun la busqueda
        /// </summary>
        public List<OMDbMovie> Search { get; set; }

        /// <summary>
        /// cantidad total de resultados pero viene como texto desde la api
        /// </summary>
        public string totalResults { get; set; }

        /// <summary>
        /// indica si la respuesta fue exitosa o no
        /// </summary>
        public bool Response { get; set; }

        /// <summary>
        /// mensaje de error si algo salio mal
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// convierte el totalResults a numero si se puede para usarlo en la logica de paginacion
        /// </summary>
        public int? totalResultsInt => int.TryParse(totalResults, out var result) ? result : (int?)null;
    }
}
