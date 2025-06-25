namespace CineAtom.Web.DTOs.Pelicula
{
    public class OMDbSearchResponseDTO
    {
        public List<PeliculaDTO> Search { get; set; } = new();
        public string TotalResults { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }
}
