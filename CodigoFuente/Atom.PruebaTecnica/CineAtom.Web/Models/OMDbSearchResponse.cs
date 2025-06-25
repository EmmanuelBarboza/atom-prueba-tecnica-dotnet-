using CineAtom.Web.Controllers;

namespace CineAtom.Web.Models
{
    public class OMDbSearchResponse
    {
        public List<OMDbMovie> Search { get; set; }
        public string totalResults { get; set; }
        public bool Response { get; set; }
        public string Error { get; set; }

        public int? totalResultsInt => int.TryParse(totalResults, out var result) ? result : (int?)null;
    }
}
