using System.Threading.Tasks;

namespace DictionarySuggestDemo.Models
{
    public interface ISuggestService
    {
        Task<string[]> GetSuggestions(string text);
    }
}
