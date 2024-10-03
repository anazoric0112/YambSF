using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;

namespace YambSheetData
{
    public interface IYambSheet : IService
    {
        Task<IEnumerable<KeyValuePair<int, Sheet>>> GetActiveSheets();

        Task<Sheet> GetUserActiveSheet(int id);

        Task<Sheet> AddMove(int id, int[][] dice);

        Task<Sheet> AddMoveToExactField(int id, int cnt, int target, string where);

        Task<Sheet> ResetGame(int id);
    }
}
