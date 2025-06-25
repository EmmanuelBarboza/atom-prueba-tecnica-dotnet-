namespace CineAtom.WebApi.DTOs.Articulo
{
    public class UpdateArticuloDTO
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }
    }
}
