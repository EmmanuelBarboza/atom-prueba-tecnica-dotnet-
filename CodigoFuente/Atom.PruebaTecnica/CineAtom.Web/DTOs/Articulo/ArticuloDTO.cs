using CineAtom.Web.Models;

namespace CineAtom.Web.DTOs.Articulo
{
    public class ArticuloDTO
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public Categoria Categoria { get; set; } 
    }
}
