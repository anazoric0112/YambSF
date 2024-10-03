using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using YambUserData;

namespace YambStatistics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YambStatisticsController : Controller
    {
        private const int yambUserDataPartitionCnt = 5;
        
        [HttpGet]
        [Route("leaderboard")]
        public async Task<int[][]> GetUserLeaderboard()
        {
            List<KeyValuePair<int,int>> userHighscores = new List<KeyValuePair<int,int>>();
            for (int i = 0; i < yambUserDataPartitionCnt; i++)
            {
                IEnumerable<KeyValuePair<int, User>> users = await GetYambUserData(i).GetAllUsers();
                foreach (KeyValuePair<int, User> user in users)
                {
                    userHighscores.Add(new KeyValuePair<int, int>(user.Value.Highscore, user.Key));
                }
            }
            userHighscores.Sort((x, y) => y.Key.CompareTo(x.Key));

            int[][] leaderboard = new int[userHighscores.Count][];
            int j = 0;

            foreach(KeyValuePair<int,int> hs in userHighscores)
            {
                int rank = 1;
                if (j>0)
                {
                    if (leaderboard[j - 1][2] == hs.Key) rank = leaderboard[j - 1][0];
                    else rank = leaderboard[j - 1][0] + 1;
                }
                leaderboard[j++] = [rank, hs.Value, hs.Key];
            }

            return leaderboard;
        }

        [HttpGet]
        [Route("ranking/{id}")]
        public async Task<int> GetUserRanking(int id)
        {
            int rank = 1;
            int highscore = (await GetYambUserData(id).GetUser(id)).Highscore;

            for (int i = 0; i < yambUserDataPartitionCnt; i++)
            {                                                                                                           
                IEnumerable<KeyValuePair<int, User>> users = await GetYambUserData(i).GetAllUsers();
                foreach (KeyValuePair<int, User> user in users)
                {
                    if (user.Key == id) continue;
                    if (user.Value.Highscore > highscore) rank++;
                }
            }
            return rank;
        }

        [HttpGet]
        [Route("highscore/{id}")]
        public async Task<int> GetHighscore(int id)
        {
            return (await GetYambUserData(id).GetUser(id)).Highscore;
        }

        [HttpGet]
        [Route("averagescore/{id}")]
        public async Task<float> GetAverage(int id)
        {
            return (await GetYambUserData(id).GetUser(id)).AverageScore;
        }

        private static long GetDataPartitionKey(int id)
        {
            return id % yambUserDataPartitionCnt;
        }
        private static IYambUserData GetYambUserData(int id)
        {
            return ServiceProxy.Create<IYambUserData>(new Uri("fabric:/Yamb/YambUserData"), new ServicePartitionKey(GetDataPartitionKey(id)));
        }
    }
}
