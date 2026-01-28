using ChronoNet.Application.DTO;
using ChronoNet.Application.Services;
using ChronoNet.Domain;
using ChronoNet.Domain.Enums;

namespace ChronoNet.Tests
{
    [TestClass]
    public sealed class ReachabilityTest
    {
        [TestMethod]
        public void CalculateReachability_WithoutCapabilities_ReturnsReachable()
        {
            List<Device> devices = new List<Device>()
            {
                new Device("a1"),
                new Device("a2"),
                new Device("b1"),
                new Device("b2")
            };

            List<TemporalGraph> temporalGraphs = new List<TemporalGraph>()
            {
                new TemporalGraph(0, new TimeInterval(1000000000, 1000000003),
                    devices.AsReadOnly(), new List<Edge>
                    {
                        new Edge(devices[0].Id, devices[2].Id, EdgeDirection.Right)
                    }),
                new TemporalGraph(0, new TimeInterval(1000000003, 1000000005),
                    devices.AsReadOnly(), new List<Edge>
                    {
                        new Edge(devices[0].Id, devices[2].Id, EdgeDirection.Left),
                        new Edge(devices[0].Id, devices[3].Id, EdgeDirection.Left)
                    }),
                new TemporalGraph(0, new TimeInterval(1000000005, 1000000010),
                    devices.AsReadOnly(), new List<Edge>
                    {
                        new Edge(devices[0].Id, devices[2].Id, EdgeDirection.Left),
                        new Edge(devices[0].Id, devices[3].Id, EdgeDirection.Left),
                        new Edge(devices[1].Id, devices[2].Id, EdgeDirection.Left)
                    }),
                new TemporalGraph(0, new TimeInterval(1000000010, 1000000012),
                    devices.AsReadOnly(), new List<Edge>
                    {
                        new Edge(devices[0].Id, devices[3].Id, EdgeDirection.Left),
                        new Edge(devices[1].Id, devices[2].Id, EdgeDirection.Left)
                    }),
                new TemporalGraph(0, new TimeInterval(1000000012, 1000000015),
                    devices.AsReadOnly(), new List<Edge>
                    {
                        new Edge(devices[1].Id, devices[2].Id, EdgeDirection.Left),
                    }),
            };

            Dictionary<string, Device> deviceMap = new Dictionary<string, Device>();
            foreach (var device in devices)
            {
                deviceMap[device.Name] = device;
            }

            ReachabilityRequest request = new ReachabilityRequest()
            {
                SourceDeviceName = "a1",
                TargetDeviceNames = new List<string>() { "b1" },
                CustomInterval = new TimeInterval(1000000000, 1000000003),
                ConsiderCapabilities = false
            };

            var result = ReachabilityService.CalculateReachability(temporalGraphs, request, deviceMap);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(1, result.AllPaths.Count);
            Assert.IsTrue(result.AllPaths[0].Path.SequenceEqual(new List<Guid> { devices[0].Id, devices[2].Id }));
            Assert.AreEqual(new TimeInterval(1000000000, 1000000003), result.AllPaths[0].Interval);
        }
    }
}
