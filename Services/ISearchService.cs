namespace SemanticSearch.Services
{
    public interface ISearchService
    {
        Task<float[]> SemanticSearch(string inputText);
    }
}
