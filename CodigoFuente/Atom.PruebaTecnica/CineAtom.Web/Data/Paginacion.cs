using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineAtom.Web.Data
{
    /// <summary>
    /// Clase generica para manejar paginacion de listas
    /// Facilita dividir los datos en paginas para mostrar en la vista
    /// </summary>
    public class Paginacion<T> : List<T>
    {
        /// <summary>
        /// Numero de la pagina actual
        /// </summary>
        public int PaginaInicio { get; private set; }

        /// <summary>
        /// Total de paginas disponibles
        /// </summary>
        public int PaginasTotales { get; private set; }

        /// <summary>
        /// Constructor que recibe los datos ya paginados
        /// </summary>
        public Paginacion(List<T> items, int contador, int paginaInicio, int cantidadregistros)
        {
            PaginaInicio = paginaInicio;
            PaginasTotales = (int)Math.Ceiling(contador / (double)cantidadregistros);
            this.AddRange(items);
        }

        /// <summary>
        /// Indica si hay paginas antes de la actual
        /// </summary>
        public bool PaginasAnteriores => PaginaInicio > 1;

        /// <summary>
        /// Indica si hay paginas despues de la actual
        /// </summary>
        public bool PaginasPosteriores => PaginaInicio < PaginasTotales;

        /// <summary>
        /// Crea la paginacion a partir de una lista completa en memoria
        ///  Para pruebas o colecciosnes que ya 
        /// </summary>
        public static Paginacion<T> CrearPaginacion(IEnumerable<T> fuente, int paginaInicio, int cantidadregistros)
        {
            var items = fuente.Skip((paginaInicio - 1) * cantidadregistros)
                              .Take(cantidadregistros)
                              .ToList();

            return new Paginacion<T>(items, fuente.Count(), paginaInicio, cantidadregistros);
        }
    }
}
