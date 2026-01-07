using ChronoNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Application.DTO
{
    public class ReachabilityRequest
    {
        public string SourceDeviceName { get; set; } = string.Empty;
        public List<string> TargetDeviceNames { get; set; } = new();
        public TimeInterval CustomInterval { get; set; }
        public bool ConsiderCapabilities { get; set; } = true;
        public long? DataSize { get; set; }
    }
}
