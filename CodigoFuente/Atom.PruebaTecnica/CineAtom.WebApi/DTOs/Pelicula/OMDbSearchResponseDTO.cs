namespace CineAtom.WebApi.DTOs.Pelicula
{
    public class OMDbSearchResponseDTO
    {
        public List<PeliculaDTO> Search { get; set; } = new();
        public string TotalResults { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }

    public class PeliculaDTO
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string imdbID { get; set; }
        public string Type { get; set; }
        public string Poster { get; set; }
    }
}
