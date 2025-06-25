namespace CineAtom.Web.Models
{
    public class Articulo
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }

}
