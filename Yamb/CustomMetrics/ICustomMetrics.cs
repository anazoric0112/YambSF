using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMetrics
{
    public interface ICustomMetrics: IService
    {
        Task<int> GetMetricValue(Metric.Name metric);
        Task<int> AddMetricValue(Metric.Name metric, int val);
        Task<int> SubMetricValue(Metric.Name metric, int val);
    }
}
