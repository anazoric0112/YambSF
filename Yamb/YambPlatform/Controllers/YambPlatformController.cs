using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;
using YambUserData;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using YambGame;

namespace YambPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YambPlatformController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly string reverseProxyBaseUri;
        private readonly StatelessServiceContext serviceContext;

        public YambPlatformController(StatelessServiceContext context)
        {
            this.fabricClient = new FabricClient();
            this.httpClient = new HttpClient();
            this.serviceContext = context;
            this.reverseProxyBaseUri = Environment.GetEnvironmentVariable("ReverseProxyBaseUri");
        }


        [HttpPost]
        [Route("playgame/{id}")]
        public async Task<int> PlayGame(int id)
        {
            return await GetYambGame(id).PlayGame(id);
        }

        [HttpPost]
        [Route("startgame/{id}")]
        public async Task<IActionResult> StartGame(int id)
        {
            await GetYambGame(id).StartGame(id);
            return Ok();
        }

        [HttpGet]
        [Route("throwdice/{id}")]
        public async Task<int[][]> ThrowDice(int id)
        {
            return await GetYambGame(id).ThrowDice(id);
        }

        [HttpGet]
        [Route("sheet/{id}")]
        public async Task<int[][]> GetSheet(int id)
        {
            return await GetYambGame(id).GetSheet(id);
        }

        [HttpPost]
        [Route("addmove/{id}/{cnt}/{target}/{where}")]
        public async Task<IActionResult> AddMove(int id, int cnt, int target, string where)
        {
            await GetYambGame(id).AddMove(id, cnt, target, where);

            return Ok();
        }

        [HttpDelete]
        [Route("resetuser/{id}")]
        public async Task<IActionResult> ResetUserData(int id)
        {
            await GetYambUserData(id).ClearUserData(id);
            return Ok();
        }

        [HttpDelete]
        [Route("resetdb")]
        public async Task<IActionResult> ResetData()
        {
            for (int i = 0; i < 5; i++)
            {
                await GetYambUserData(i).ClearDatabase();
            }
            return Ok();
        }

        [HttpGet]
        [Route("highscore/{id}")]
        public async Task<int> GetHighscore(int id)
        {
            Uri serviceName = YambPlatform.GetServiceName(this.serviceContext, "YambStatistics");
            Uri proxyAddress = this.GetReverseProxyAddress(serviceName);

            string proxyUrl = $"{proxyAddress}/api/YambStatistics/highscore/{id}";
            using HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl);

            return int.Parse(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        [Route("averagescore/{id}")]
        public async Task<float> GetAveragescore(int id)
        {
            Uri serviceName = YambPlatform.GetServiceName(this.serviceContext, "YambStatistics");
            Uri proxyAddress = this.GetReverseProxyAddress(serviceName);

            string proxyUrl = $"{proxyAddress}/api/YambStatistics/averagescore/{id}";
            using HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl);

            return float.Parse(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        [Route("ranking/{id}")]
        public async Task<int> GetRanking(int id)
        {
            Uri serviceName = YambPlatform.GetServiceName(this.serviceContext, "YambStatistics");
            Uri proxyAddress = this.GetReverseProxyAddress(serviceName);

            string proxyUrl = $"{proxyAddress}/api/YambStatistics/ranking/{id}";
            using HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl);

            return int.Parse(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        [Route("leaderboard")]
        public async Task<int[][]> GetLeaderboard()
        {
            Uri serviceName = YambPlatform.GetServiceName(this.serviceContext, "YambStatistics");
            Uri proxyAddress = this.GetReverseProxyAddress(serviceName);

            string proxyUrl = $"{proxyAddress}/api/YambStatistics/leaderboard";
            using HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl);

            string resultString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<int[][]>(resultString);
        }

        private static long GetDataPartitionKey(int id)
        {
            return id % 5;
        }

        private static IYambUserData GetYambUserData(int id)
        {
            return ServiceProxy.Create<IYambUserData>(new Uri("fabric:/Yamb/YambUserData"), new ServicePartitionKey(GetDataPartitionKey(id)));
        }

        private static IYambGame GetYambGame(int id)
        {
            return ServiceProxy.Create<IYambGame>(new Uri("fabric:/Yamb/YambGame"));
        }

        private Uri GetReverseProxyAddress(Uri serviceName)
        {
            return new Uri($"{this.reverseProxyBaseUri}{serviceName.AbsolutePath}");
        }

    }
}
