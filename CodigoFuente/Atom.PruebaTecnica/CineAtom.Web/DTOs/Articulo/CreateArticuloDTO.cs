
namespace CineAtom.Web.DTOs.Articulo
{
    public class CreateArticuloDTO
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }
    }
}
