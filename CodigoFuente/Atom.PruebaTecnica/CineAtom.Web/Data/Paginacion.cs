using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CineAtom.Web.Data
{
    public class Paginacion<T> : List<T>
    {
        public int PaginaInicio { get; private set; }
        public int PaginasTotales { get; private set; }

        public Paginacion(List<T> items, int contador, int paginaInicio, int cantidadregistros)
        {
            PaginaInicio = paginaInicio;
            PaginasTotales = (int)Math.Ceiling(contador / (double)cantidadregistros);

            this.AddRange(items);
        }

        public bool PaginasAnteriores => PaginaInicio > 1;
        public bool PaginasPosteriores => PaginaInicio < PaginasTotales;

        // Versión síncrona para colecciones en memoria
        public static Paginacion<T> CrearPaginacion(IEnumerable<T> fuente, int paginaInicio, int cantidadregistros)
        {
            var items = fuente.Skip((paginaInicio - 1) * cantidadregistros)
                             .Take(cantidadregistros)
                             .ToList();

            return new Paginacion<T>(items, fuente.Count(), paginaInicio, cantidadregistros);
        }
    }
}