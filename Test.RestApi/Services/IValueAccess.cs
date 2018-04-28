using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.RestApi
{
    public interface IValueAccess
    {
        Task<string> AddAsync(RequestDto requestDto);
        Task<IList<ResponseDto>> GetAsync();
    }
}