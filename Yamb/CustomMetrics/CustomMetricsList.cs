using System.Collections.ObjectModel;
using System.Fabric.Description;

namespace CustomMetrics
{
    public class CustomMetricsList : KeyedCollection<string, ServiceLoadMetricDescription>
    {
        protected override string GetKeyForItem(ServiceLoadMetricDescription item)
        {
            return item.Name;
        }
    }
}
