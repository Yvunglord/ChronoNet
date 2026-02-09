using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Flows;
using DynamicNetwork.Domain.Functions;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.SynthesisTester;

public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public List<TemporalGraph> Graphs { get; set; } = new();
    public List<DataFlow> Flows { get; set; } = new();
    public FunctionLibrary Library { get; set; } = null!;
    public Dictionary<NodeConfiguration, TimeInterval> NodeInputs { get; set; } = new();
    public List<NodeConfiguration> OutputNodes { get; set; } = new();
    public TimeInterval CustomInterval { get; set; }
}

public class TestScenarios
{
    public string GetScenarioName(int id)
    {
        return id switch
        {
            1 => "Простой видео-поток (4K → 1080p → H.264)",
            2 => "Два параллельных потока (видео + аудио)",
            3 => "Сеть с изолированными узлами",
            4 => "Минимальная сеть (2 узла, 1 связь)",
            5 => "Сложная сеть с множественными путями",
            6 => "Тестовый файлик",
            _ => "Неизвестный сценарий"
        };
    }

    public TestScenario GetScenario(int id)
    {
        return id switch
        {
            1 => CreateSimpleVideoScenario(),
            2 => CreateParallelStreamsScenario(),
            3 => CreateNetworkWithIsolatedNodesScenario(),
            4 => CreateMinimalNetworkScenario(),
            5 => CreateComplexNetworkScenario(),
            6 => CreateOutputFileTestScenario(),
            _ => throw new ArgumentException($"Неизвестный сценарий: {id}")
        };
    }

    private TestScenario CreateSimpleVideoScenario()
    {
        var library = new FunctionLibrary(
            new[]
            {
                new ProcessType("decode_4k", 0.1, "video_4k", "video_1080p", 1.0),
                new ProcessType("encode_h264", 0.2, "video_1080p", "video_h264", 1.0)
            },
            new[]
            {
                new TransportType("fiber", 0.05, "video_1080p", 100.0),
                new TransportType("ethernet", 0.1, "video_h264", 50.0)
            },
            new[]
            {
                new StorageType("buffer", new[] { "video_1080p", "video_h264" })
            }
        );

        var graphs = new List<TemporalGraph>
        {
            new TemporalGraph(0, new TimeInterval(0, 100), new[]
            {
                new Link("source", "node1", LinkDirection.Right)
            }, new[] { "source", "node1", "sink" }),

            new TemporalGraph(1, new TimeInterval(100, 200), new[]
            {
                new Link("node1", "node2", LinkDirection.Right)
            }, new[] { "source", "node1", "node2", "sink" }),

            new TemporalGraph(2, new TimeInterval(200, 300), new[]
            {
                new Link("node2", "sink", LinkDirection.Right)
            }, new[] { "source", "node1", "node2", "sink" })
        };

        var flows = new List<DataFlow>
        {
            new DataFlow(
                "video_stream",
                100.0,
                new[]
                {
                    new FlowTransformation("video_4k", "video_1080p"),
                }),
            new DataFlow(
                "video_1080p",
                100.0,
                new[]
                {
                    new FlowTransformation("video_1080p", "video_h264")
                }),
            new DataFlow(
                "video_h264",
                100.0,
                new List<FlowTransformation>()
                )
        };

        var sourceNode = new NodeConfiguration(
            "source",
            new[] { "decode_4k" },
            new[] { "video_4k" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var sinkNode = new NodeConfiguration(
            "sink",
            new[] { "encode_h264" },
            new string[] { },
            new[] { "video_h264" },
            new Dictionary<string, double>(),
            new string[] { });

        return new TestScenario
        {
            Name = "Простой видео-поток",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval> { { sourceNode, new TimeInterval(0, 100) } },
            OutputNodes = new List<NodeConfiguration> { sinkNode },
            CustomInterval = new TimeInterval(0, 300)
        };
    }

    private TestScenario CreateParallelStreamsScenario()
    {
        var library = new FunctionLibrary(
            new[]
            {
                new ProcessType("decode_video", 0.1, "video_raw", "video_proc", 1.0),
                new ProcessType("decode_audio", 0.05, "audio_raw", "audio_proc", 1.0),
                new ProcessType("mux", 0.2, "video_proc", "stream", 1.0),
                new ProcessType("mux", 0.2, "audio_proc", "stream", 1.0)
            },
            new[]
            {
                new TransportType("video_link", 0.1, "video_proc", 200.0),
                new TransportType("audio_link", 0.1, "audio_proc", 50.0),
                new TransportType("stream_link", 0.1, "stream", 250.0)
            },
            new[]
            {
                new StorageType("media_buffer", new[] { "video_proc", "audio_proc", "stream" })
            }
        );

        var graphs = new List<TemporalGraph>
        {
            new TemporalGraph(0, new TimeInterval(0, 150), new[]
            {
                new Link("camera", "processor", LinkDirection.Right),
                new Link("mic", "processor", LinkDirection.Right)
            }, new[] { "camera", "mic", "processor", "broadcaster" }),

            new TemporalGraph(1, new TimeInterval(150, 300), new[]
            {
                new Link("processor", "broadcaster", LinkDirection.Right)
            }, new[] { "camera", "mic", "processor", "broadcaster" })
        };

        var flows = new List<DataFlow>
        {
            new DataFlow("video_flow", 80.0, new[]
            {
                new FlowTransformation("video_raw", "video_proc")
            }),
            new DataFlow("audio_flow", 20.0, new[]
            {
                new FlowTransformation("audio_raw", "audio_proc")
            })
        };

        var cameraNode = new NodeConfiguration(
            "camera",
            new[] { "decode_video" },
            new[] { "video_raw" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var micNode = new NodeConfiguration(
            "mic",
            new[] { "decode_audio" },
            new[] { "audio_raw" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var broadcasterNode = new NodeConfiguration(
            "broadcaster",
            new[] { "mux" },
            new string[] { },
            new[] { "stream" },
            new Dictionary<string, double>(),
            new string[] { });

        return new TestScenario
        {
            Name = "Два параллельных потока",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval>
            {
                { cameraNode, new TimeInterval(0, 150) },
                { micNode, new TimeInterval(0, 150) }
            },
            OutputNodes = new List<NodeConfiguration> { broadcasterNode },
            CustomInterval = new TimeInterval(0, 300)
        };
    }

    private TestScenario CreateNetworkWithIsolatedNodesScenario()
    {
        var library = new FunctionLibrary(
            new[] { new ProcessType("process", 0.1, "data", "result", 1.0) },
            new[] { new TransportType("link", 0.1, "data", 100.0) },
            new[] { new StorageType("storage", new[] { "data", "result" }) }
        );

        var graphs = new List<TemporalGraph>
        {
            new TemporalGraph(0, new TimeInterval(0, 200), new[]
            {
                new Link("A", "B", LinkDirection.Right),
                new Link("B", "C", LinkDirection.Right)
            }, new[] { "A", "B", "C", "D" })
        };

        var flows = new List<DataFlow>
        {
            new DataFlow("data_flow", 50.0, new[]
            {
                new FlowTransformation("data", "result")
            })
        };

        var nodeA = new NodeConfiguration(
            "A",
            new[] { "process" },
            new[] { "data" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var nodeC = new NodeConfiguration(
            "C",
            new[] { "process" },
            new string[] { },
            new[] { "result" },
            new Dictionary<string, double>(),
            new string[] { });

        return new TestScenario
        {
            Name = "Сеть с изолированными узлами",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval> { { nodeA, new TimeInterval(0, 200) } },
            OutputNodes = new List<NodeConfiguration> { nodeC },
            CustomInterval = new TimeInterval(0, 200)
        };
    }

    private TestScenario CreateMinimalNetworkScenario()
    {
        var library = new FunctionLibrary(
            new[] { new ProcessType("simple_proc", 0.1, "input", "output", 1.0) },
            new[] { new TransportType("simple_link", 0.1, "input", 10.0) },
            new[] { new StorageType("simple_storage", new[] { "input", "output" }) }
        );

        var graphs = new List<TemporalGraph>
        {
            new TemporalGraph(0, new TimeInterval(0, 100), new[]
            {
                new Link("source", "sink", LinkDirection.Right)
            }, new[] { "source", "sink" })
        };

        var flows = new List<DataFlow>
        {
            new DataFlow("simple_flow", 5.0, new[]
            {
                new FlowTransformation("input", "output")
            })
        };

        var sourceNode = new NodeConfiguration(
            "source",
            new[] { "simple_proc" },
            new[] { "input" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var sinkNode = new NodeConfiguration(
            "sink",
            new[] { "simple_proc" },
            new string[] { },
            new[] { "output" },
            new Dictionary<string, double>(),
            new string[] { });

        return new TestScenario
        {
            Name = "Минимальная сеть",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval> { { sourceNode, new TimeInterval(0, 100) } },
            OutputNodes = new List<NodeConfiguration> { sinkNode },
            CustomInterval = new TimeInterval(0, 100)
        };
    }

    private TestScenario CreateComplexNetworkScenario()
    {
        var library = new FunctionLibrary(
            new[]
            {
                new ProcessType("proc1", 0.1, "type1", "type2", 1.0),
                new ProcessType("proc2", 0.1, "type2", "type3", 1.0)
            },
            new[]
            {
                new TransportType("t1", 0.1, "type1", 50.0),
                new TransportType("t2", 0.1, "type2", 50.0),
                new TransportType("t3", 0.1, "type3", 50.0)
            },
            new[] { new StorageType("s1", new[] { "type1", "type2", "type3" }) }
        );

        var graphs = new List<TemporalGraph>
        {
            new TemporalGraph(0, new TimeInterval(0, 100), new[]
            {
                new Link("A", "B", LinkDirection.Right),
                new Link("A", "C", LinkDirection.Right),
                new Link("B", "D", LinkDirection.Right),
                new Link("C", "D", LinkDirection.Right)
            }, new[] { "A", "B", "C", "D", "E" }),

            new TemporalGraph(1, new TimeInterval(100, 200), new[]
            {
                new Link("D", "E", LinkDirection.Right),
                new Link("B", "E", LinkDirection.Right)
            }, new[] { "A", "B", "C", "D", "E" })
        };

        var flows = new List<DataFlow>
        {
            new DataFlow("flow1", 30.0, new[]
            {
                new FlowTransformation("type1", "type2"),
                new FlowTransformation("type2", "type3")
            })
        };

        var nodeA = new NodeConfiguration(
            "A",
            new[] { "proc1" },
            new[] { "type1" },
            new string[] { },
            new Dictionary<string, double>(),
            new string[] { });

        var nodeE = new NodeConfiguration(
            "E",
            new[] { "proc2" },
            new string[] { },
            new[] { "type3" },
            new Dictionary<string, double>(),
            new string[] { });

        return new TestScenario
        {
            Name = "Сложная сеть",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval> { { nodeA, new TimeInterval(0, 100) } },
            OutputNodes = new List<NodeConfiguration> { nodeE },
            CustomInterval = new TimeInterval(0, 200)
        };
    }

    private TestScenario CreateOutputFileTestScenario()
    {
        var library = new FunctionLibrary(
            new[]
            {
                new ProcessType("1", 0.2, "1", "2", 1.0),
                new ProcessType("2", 0.04, "1", "2", 1.0),
                new ProcessType("3", 0.06666666666666667, "1", "2", 1.0),
                new ProcessType("4", 0.02, "1", "2", 1.0)
            },
            new[]
            {
                new TransportType("1", 1.0, "2", 20.0),
                new TransportType("2", 1.0, "1", 20.0)
            },
            new[]
            {
                new StorageType("1", new[] { "1", "2" })
        });

        var flows = new List<DataFlow>
        {
            new DataFlow("1", 1.0, new[] { new FlowTransformation("1", "2") }),
            new DataFlow("2", 1.0, new[] { new FlowTransformation("1", "2") })
        };

        var graphs = new List<TemporalGraph>();
        var allNetworkNodes = new List<string>
        {
            "2", "3", "4", "5", "7"
        };

        var links1 = new List<Link>
        {
            new Link("2", "4", LinkDirection.Undirected),
            new Link("3", "4", LinkDirection.Undirected),
            new Link("5", "4", LinkDirection.Undirected),
            new Link("7", "2", LinkDirection.Undirected),
            new Link("7", "4", LinkDirection.Undirected)
        };
        graphs.Add(new TemporalGraph(0, new TimeInterval(0, 36), links1, allNetworkNodes));

        var links2 = new List<Link>(links1);
        graphs.Add(new TemporalGraph(1, new TimeInterval(36, 54), links2, allNetworkNodes));

        var configs = new List<StructConfiguration>();

        var nodes1 = new List<NodeConfiguration>
        {
            new NodeConfiguration("2", new[] { "1" }, new string[] { }, new[] { "2" }, new Dictionary<string, double> { { "1", 20.0 } }, new List<string>()),
            new NodeConfiguration("3", new[] { "3" }, new[] { "1" }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>()),
            new NodeConfiguration("4", new[] { "2" }, new string[] { }, new string[] { }, new Dictionary<string, double> { { "1", 50.0 } }, new List<string>()),
            new NodeConfiguration("5", new[] { "4" }, new[] { "1" }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>()),
            new NodeConfiguration("7", new[] { "2" }, new string[] { }, new[] { "1", "2" }, new Dictionary<string, double> { { "1", 80.0 } }, new List<string>())
        };
        var configLinks1 = new List<LinkConfiguration>
        {
            new LinkConfiguration("2", "4", new[] { "1", "2" }, new List<string>()),
            new LinkConfiguration("3", "4", new[] { "1", "2" }, new List<string>()),
            new LinkConfiguration("5", "4", new[] { "1", "2" }, new List<string>()),
            new LinkConfiguration("7", "2", new[] { "1", "2" }, new List<string>()),
            new LinkConfiguration("7", "4", new[] { "1", "2" }, new List<string>())
        };

        configs.Add(new StructConfiguration(new TimeInterval(0, 36), nodes1, configLinks1));

        var nodes2 = new List<NodeConfiguration>
        {
            new NodeConfiguration("2", new[] { "1" }, new string[] { }, new string[] { }, new Dictionary<string, double> { { "1", 20.0 } }, new List<string>()),
            new NodeConfiguration("3", new[] { "3" }, new string[] { }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>()),
            new NodeConfiguration("4", new[] { "2" }, new string[] { }, new string[] { }, new Dictionary<string, double> { { "1", 50.0 } }, new List<string>()),
            new NodeConfiguration("5", new[] { "4" }, new string[] { }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>()),
            new NodeConfiguration("7", new[] { "2" }, new string[] { }, new[] { "1", "2" }, new Dictionary<string, double> { { "1", 80.0 } }, new List<string>())
        };

        var configLinks2 = new List<LinkConfiguration>(configLinks1.Select(l => new LinkConfiguration(l.NodeA, l.NodeB, l.EnabledTransports, new List<string>())));

        configs.Add(new StructConfiguration(new TimeInterval(36, 54), nodes2, configLinks2));

        var node3Config = new NodeConfiguration("3", new[] { "3" }, new[] { "1" }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>());
        var node5Config = new NodeConfiguration("5", new[] { "4" }, new[] { "1" }, new string[] { }, new Dictionary<string, double> { { "1", 70.0 } }, new List<string>());
        var node7Config = new NodeConfiguration("7", new[] { "2" }, new string[] { }, new[] { "1", "2" }, new Dictionary<string, double> { { "1", 80.0 } }, new List<string>());

        return new TestScenario
        {
            Name = "Тестовый",
            Graphs = graphs,
            Flows = flows,
            Library = library,
            NodeInputs = new Dictionary<NodeConfiguration, TimeInterval>
            {
                { node3Config, new TimeInterval(0, 36) },
                { node5Config, new TimeInterval(0, 36) }
            },
            OutputNodes = new List<NodeConfiguration> { node7Config },
            CustomInterval = new TimeInterval(0, 54)
        };
    }
}