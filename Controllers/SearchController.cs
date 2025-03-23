using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SemanticSearch.Services;

namespace SemanticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [Route("search")]
        [HttpGet]
        public async Task<ActionResult> EmanticSearch(string inputText)
        {
            var response = await _searchService.SemanticSearch(inputText);
            return Ok(response);
        }
    }
}
