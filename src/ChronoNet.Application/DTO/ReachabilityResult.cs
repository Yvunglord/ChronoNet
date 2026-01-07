using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Application.DTO
{
    public class ReachabilityResult
    {
        public bool IsReachable { get; set; }
        public List<PathWithInterval> AllPaths { get; set; } = new List<PathWithInterval>();
        public int? ShortestPathLength { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<Guid, string> CapabilityIssues { get; set; } = new();
    }
}
