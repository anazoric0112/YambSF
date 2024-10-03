using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace YambUserData
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class YambUserData : StatefulService, IYambUserData
    {
        string userDictName = "UserDataDictionary";

        public YambUserData(StatefulServiceContext context)
            : base(context)
        { }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return base.RunAsync(cancellationToken);
        }


        public async Task<IEnumerable<KeyValuePair<int, User>>> GetAllUsers()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, User> userDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);
            List<KeyValuePair<int, User>> users = new List<KeyValuePair<int, User>>();

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, User>> enumerable = await userDict.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, User>> enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    users.Add(enumerator.Current);
                }

                await tx.CommitAsync();
            }

            return users;
        }

        public async Task<User> GetUser(int id)
        {
            IReliableDictionary<int, User> userDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);
            User user = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                user = await userDict.GetOrAddAsync(tx, id, new User(id));
                await tx.CommitAsync();
            }

            return user;
        }

        public async Task<int> GetHighscore(int id)
        {
            IReliableDictionary<int, User> userDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);
            User user = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                user = await userDict.GetOrAddAsync(tx, id, new User(id));
                await tx.CommitAsync();
            }

            return user.Highscore;
        }

        public async Task<User> AddScore(int id, int score)
        {
            IReliableDictionary<int, User> userDict = 
                        await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);
            User user = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                user = await userDict.AddOrUpdateAsync(tx, id, 
                                        (key) => new User(key, score), 
                                        (key, oldVal) => oldVal.AddScore(score));
                await tx.CommitAsync();
            }
            return user;
        }

        public async Task<int> ClearDatabase()
        {
            IReliableDictionary<int, User> userDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                await userDict.ClearAsync();
                await tx.CommitAsync();
            }
            return 1;
        }

        public async Task<int> ClearUserData(int id)
        {
            IReliableDictionary<int, User> userDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, User>>(userDictName);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                await userDict.SetAsync(tx, id, new User(id));
                await tx.CommitAsync();
            }
            return 1;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
