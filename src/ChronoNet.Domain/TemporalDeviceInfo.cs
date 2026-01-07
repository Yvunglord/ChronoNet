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

        public TemporalDeviceInfo(Guid id, LocalCapabilities localCapabilities = LocalCapabilities.None)
        {
            Id = id;
            LocalCapabilities = localCapabilities;
        }
    }
}
