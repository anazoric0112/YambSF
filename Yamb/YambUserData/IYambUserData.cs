using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Fabric.Query;

namespace YambUserData
{
    public interface IYambUserData : IService
    {

        Task<IEnumerable<KeyValuePair<int, User>>> GetAllUsers();

        Task<User> GetUser(int id);

        Task<User> AddScore(int id, int score);

        Task<int> ClearDatabase();

        Task<int> ClearUserData(int id);
    }
}
