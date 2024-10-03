using CustomMetrics;
using DiceThrower.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using YambSheetData;

namespace YambGame
{
    public interface IYambGame : IService
    {
        Task ReportMetric(Metric.Name metric);

        Task<int> PlayGame(int id);

        Task<int[][]> StartGame(int id);

        Task<int[][]> ThrowDice(int id);

        Task<int[][]> GetSheet(int id);

        Task<int[][]> AddMove(int id, int cnt, int target, string where);
    }
}
