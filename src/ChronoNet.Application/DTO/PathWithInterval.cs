using ChronoNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Application.DTO
{
    public class PathWithInterval
    {
        public List<Guid> Path { get; set; } = new List<Guid>();
        public TimeInterval Interval { get; set; }
        public string FormattedPath { get; set; } = string.Empty;
    }
}
