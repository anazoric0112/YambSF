using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMetrics
{
    public class Metric
    {
        public enum Name {
            ActiveSheets
        }

        static public string MetricNameToString(Name metric)
        {
            if (metric == Name.ActiveSheets) return "ActiveSheets";

            return "";
        }
    }
}
