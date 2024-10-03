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
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace CustomMetrics
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class CustomMetrics : StatefulService, ICustomMetrics
    {
        string metricsDictName = "MetricsDataDictionary";

        public CustomMetrics(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<int> GetMetricValue(Metric.Name metric)
        {
            IReliableDictionary<string, int> metricsDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(metricsDictName);
            int result = 0;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                result = await metricsDict.GetOrAddAsync(tx, Metric.MetricNameToString(metric), 0);
                await tx.CommitAsync();
            }
            return result;
        }

        public async Task<int> AddMetricValue(Metric.Name metric, int val)
        {
            IReliableDictionary<string, int> metricsDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(metricsDictName);
            int result = 0;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                result = await metricsDict.AddOrUpdateAsync(tx, Metric.MetricNameToString(metric), 1, (key, oldVal) => oldVal + val);
                ReportMetric(metric, result);
                await tx.CommitAsync();
            }
            return result;
        }

        public async Task<int> SubMetricValue(Metric.Name metric, int val)
        {
            IReliableDictionary<string, int> metricsDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(metricsDictName);
            int result = 0;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                result = await metricsDict.AddOrUpdateAsync(tx, Metric.MetricNameToString(metric), 0, (key, oldVal) => int.Max( oldVal - val, 0));
                ReportMetric(metric, result);
                await tx.CommitAsync();
            }
            return result;
        }
        private void ReportMetric(Metric.Name metric, int val)
        {
            string metricName = Metric.MetricNameToString(metric);

            var loadMetrics = new List<LoadMetric>
            {
                new LoadMetric(metricName, val)
            };

            ServiceEventSource.Current.ServiceMessage(this.Context, $"Reported custom metric: {metricName} with value {val}");
            Partition.ReportLoad(loadMetrics);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

    }
}
