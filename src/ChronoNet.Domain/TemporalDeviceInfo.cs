using ChronoNet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Domain
{
    public class TemporalDeviceInfo
    {
        public Guid Id { get; }
        public LocalCapabilities LocalCapabilities { get; set; }

        public Dictionary<int, double> StoredFlows { get; set; } = new();
        public Dictionary<int, double> IncomingFlows { get; set; } = new();
        public Dictionary<int, double> OutgoingFlows { get; set; } = new();

        public TemporalDeviceInfo(Guid id, LocalCapabilities localCapabilities = LocalCapabilities.None)
        {
            Id = id;
            LocalCapabilities = localCapabilities;
        }


        public void AddIncoming(int flowId, double amount)
        {
            if (!IncomingFlows.ContainsKey(flowId))
                IncomingFlows[flowId] = 0;
            IncomingFlows[flowId] += amount;
        }

        public void AddOutgoing(int flowId, double amount)
        {
            if (!OutgoingFlows.ContainsKey(flowId))
                OutgoingFlows[flowId] = 0;
            OutgoingFlows[flowId] += amount;
        }
    }
}
