using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using CustomMetrics;
using System.Collections.ObjectModel;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using DiceThrower.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Client;
using YambSheetData;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;


namespace YambGame
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    public sealed class YambGame : StatelessService, IYambGame
    {
        public YambGame(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<int> PlayGame(int id)
        {
            IDiceThrower diceThrower = ActorProxy.Create<IDiceThrower>(new ActorId(id), new Uri("fabric:/Yamb/DiceThrowerActorService"));
            IYambSheet yambSheetData = GetSheetYambData(id);
            Sheet sheet = await yambSheetData.ResetGame(id);

            for (int i = 0; i < 12; i++)
            {
                int[][] dice = await diceThrower.ThrowDice(3, new CancellationToken());
                sheet = await yambSheetData.AddMove(id, dice);
                await ReportMetric(Metric.Name.ActiveSheets);
            }

            if (sheet == null) return 0;
            return sheet.GetScore();
        }

        public async Task<int[][]> StartGame(int id)
        {
            return (await GetSheetYambData(id).ResetGame(id)).SerializationArray();
        }

        public async Task<int[][]> ThrowDice(int id)
        {
            IDiceThrower diceThrower = ActorProxy.Create<IDiceThrower>(new ActorId(id), new Uri("fabric:/Yamb/DiceThrowerActorService"));
            return await diceThrower.ThrowDice(1, new CancellationToken());
        }

        public async Task<int[][]> GetSheet(int id)
        {
            return (await GetSheetYambData(id).GetUserActiveSheet(id)).SerializationArray();
        }

        public async Task<int[][]> AddMove(int id, int cnt, int target, string where)
        {
            if ((where != "up" && where != "down")
                || (target < 1 || target > 6)
                || (cnt < 0 || cnt > 5)) return null;

            int[][] ret = (await GetSheetYambData(id).AddMoveToExactField(id, cnt, target, where)).SerializationArray();

            await ReportMetric(Metric.Name.ActiveSheets);

            return ret;
        }

        private static long GetSheetDataPartitionKey(int id)
        {
            return id % 10;
        }

        private static IYambSheet GetSheetYambData(int id)
        {
            return ServiceProxy.Create<IYambSheet>(new Uri("fabric:/Yamb/YambSheetData"), new ServicePartitionKey(GetSheetDataPartitionKey(id)));
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        //--------------------------------------------------
        //-----Handling autoscaling and custom metrics------
        //--------------------------------------------------


        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await DefineDescription();
            ServiceEventSource.Current.ServiceMessage(this.Context, $"{this.Context.NodeContext.NodeName} opened with service: YambGame");
            await base.OnOpenAsync(cancellationToken);
        }

        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new StatelessServiceUpdateDescription();

            AddActiveSheetMetrics(updateDescription);
            AddActiveSheetAutoscaling(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void AddActiveSheetMetrics(StatelessServiceUpdateDescription updateDescription)
        {
            var userLoadMetric = new StatelessServiceLoadMetricDescription
            {
                Name = Metric.MetricNameToString(Metric.Name.ActiveSheets),
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            updateDescription.Metrics = new CustomMetricsList();
            updateDescription.Metrics.Add(userLoadMetric);                                                                                                       
        }

        private void AddActiveSheetAutoscaling(StatelessServiceUpdateDescription updateDescription)
        {
            PartitionInstanceCountScaleMechanism scaleMechanism = new PartitionInstanceCountScaleMechanism
            {
                MinInstanceCount = 1,
                MaxInstanceCount = 5,
                ScaleIncrement = 1
            };

            AveragePartitionLoadScalingTrigger trigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = Metric.MetricNameToString(Metric.Name.ActiveSheets),
                LowerLoadThreshold = 2.0,
                UpperLoadThreshold = 4.0,
                ScaleInterval = TimeSpan.FromSeconds(60)
            };

            ScalingPolicyDescription scalingPolicyDescription = new ScalingPolicyDescription(scaleMechanism, trigger);

            updateDescription.ScalingPolicies ??= new List<ScalingPolicyDescription>();
            updateDescription.ScalingPolicies.Add(scalingPolicyDescription);
        }

        public async Task ReportMetric(Metric.Name metric)
        {
            int val = await GetMetricsData().GetMetricValue(metric);
            string metricName = Metric.MetricNameToString(metric);

            var loadMetrics = new List<LoadMetric>
            {
                new LoadMetric(metricName, val)
            };

            ServiceEventSource.Current.ServiceMessage(this.Context, $"Reported custom metric: {metricName} with value {val}");
            Partition.ReportLoad(loadMetrics);
        }

        private static ICustomMetrics GetMetricsData()
        {
            return ServiceProxy.Create<ICustomMetrics>(new Uri("fabric:/Yamb/CustomMetrics"));
        }

    }
}
