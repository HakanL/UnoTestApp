using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace DMXCore.DMXCore100.Extensions;

public class SimpleClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue sourceContextValue) &&
            sourceContextValue is ScalarValue scalarValue &&
            scalarValue.Value is string fullName)
        {
            string className = fullName.Substring(fullName.LastIndexOf('.') + 1);
            var classProperty = new LogEventProperty("SourceContext", new ScalarValue(className));
            logEvent.AddOrUpdateProperty(classProperty);

#if DEBUG
            classProperty = new LogEventProperty("FullSourceContext", new ScalarValue(fullName));
            logEvent.AddOrUpdateProperty(classProperty);
#endif
        }
    }
}
