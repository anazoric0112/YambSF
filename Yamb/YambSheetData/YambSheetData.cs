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
using System.Text;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using YambUserData;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using CustomMetrics;

namespace YambSheetData
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class YambSheetData : StatefulService, IYambSheet
    {
        public YambSheetData(StatefulServiceContext context)
            : base(context)
        { }

        string sheetDictName = "UserSheetDictionary";

        public async Task<IEnumerable<KeyValuePair<int, Sheet>>> GetActiveSheets()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, Sheet> sheetDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, Sheet>>(sheetDictName);
            List<KeyValuePair<int, Sheet>> sheets = new List<KeyValuePair<int, Sheet>>();

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, Sheet>> enumerable = await sheetDict.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, Sheet>> enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    sheets.Add(enumerator.Current);
                }

                await tx.CommitAsync();
            }

            return sheets;
        }

        public async Task<Sheet> GetUserActiveSheet(int id)
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, Sheet> sheetDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, Sheet>>(sheetDictName);
            Sheet sheet = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                sheet = await sheetDict.GetOrAddAsync(tx, id, new Sheet());
                await tx.CommitAsync();
            }

            return sheet;
        }

        public async Task<Sheet> AddMove(int id, int[][] dice)
        {
            IReliableDictionary<int, Sheet> sheetDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, Sheet>>(sheetDictName);
            Sheet sheet = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<Sheet> condSheet = await sheetDict.TryGetValueAsync(tx, id);
                if (!condSheet.HasValue || !condSheet.Value.InUse)
                {
                    await AddSheetToMetric();
                }

                sheet = await sheetDict.AddOrUpdateAsync(tx, id, new Sheet().WriteMove(dice), (key, oldSheet) => oldSheet.WriteMove(dice));

                if (sheet.Complete)
                {
                    await WriteFinalScore(id, sheet);
                    await sheetDict.TryRemoveAsync(tx, id);
                    await RemoveSheetFromMetric();
                }

                await tx.CommitAsync();
            }

            return sheet;
        }

        public async Task<Sheet> AddMoveToExactField(int id, int cnt, int target, string where)
        {
            IReliableDictionary<int, Sheet> sheetDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, Sheet>>(sheetDictName);
            Sheet sheet = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<Sheet> condSheet = await sheetDict.TryGetValueAsync(tx, id);
                if (!condSheet.HasValue || !condSheet.Value.InUse)
                {
                    await AddSheetToMetric();
                }

                sheet = await sheetDict.AddOrUpdateAsync(tx, id, 
                        new Sheet().WriteMoveToField(cnt, target, where),
                        (key, oldSheet) => oldSheet.WriteMoveToField(cnt, target, where));

                if (sheet.Complete)
                {
                    await WriteFinalScore(id, sheet);
                    await sheetDict.TryRemoveAsync(tx, id);
                    await RemoveSheetFromMetric();
                }

                await tx.CommitAsync();
            }

            return sheet;
        }

        public async Task<Sheet> ResetGame(int id)
        {
            IReliableDictionary<int, Sheet> sheetDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, Sheet>>(sheetDictName);
            Sheet sheet = null;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                sheet = await sheetDict.AddOrUpdateAsync(tx, id, new Sheet(), (key, old) => old.Clear());
                await tx.CommitAsync();
            }

            return sheet;
        }

        private async Task WriteFinalScore(int id, Sheet sheet)
        {
            IYambUserData YambUserData = GetYambUserData(id);
            await YambUserData.AddScore(id, sheet.GetScore());
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        private static IYambUserData GetYambUserData(int id)
        {
            return ServiceProxy.Create<IYambUserData>(new Uri("fabric:/Yamb/YambUserData"), 
                                                      new ServicePartitionKey(GetDataPartitionKey(id)));
        }

        private static long GetDataPartitionKey(int id)
        {
            return id % 5;
        }

        private static ICustomMetrics GetMetricsData()
        {
           return ServiceProxy.Create<ICustomMetrics>(new Uri("fabric:/Yamb/CustomMetrics"));
        }

        private static async Task AddSheetToMetric()
        {
            await GetMetricsData().AddMetricValue(Metric.Name.ActiveSheets, 1);
        }

        private static async Task RemoveSheetFromMetric()
        {
           await GetMetricsData().SubMetricValue(Metric.Name.ActiveSheets, 1);
        }

    }
}
