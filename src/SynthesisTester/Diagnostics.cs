using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.UseCases;
using DynamicNetwork.Application.Interfaces.UseCases.Reachability;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using Microsoft.Extensions.DependencyInjection;
namespace DynamicNetwork.SynthesisTester;

public class Diagnostics
{
    private readonly IServiceProvider _services;

    public Diagnostics(IServiceProvider services)
    {
        _services = services;
    }

    public async Task RunPreSynthesisDiagnosticsAsync(TestScenario scenario)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🔍 ДИАГНОСТИКА ДО СИНТЕЗА");
        Console.ResetColor();

        // 1. Проверка библиотеки функций
        var libraryProvider = _services.GetRequiredService<IFunctionLibraryProvider>();
        var library = libraryProvider.GetCurrent();
        Console.WriteLine($"   • Библиотека: {library.Processes.Count} процессов, {library.Transports.Count} транспортов");

        // 2. Проверка графов
        Console.WriteLine($"   • Временные графы: {scenario.Graphs.Count}");
        foreach (var graph in scenario.Graphs)
        {
            Console.WriteLine($"     - Интервал [{graph.Interval.Start}, {graph.Interval.End}]: " +
                             $"{graph.Links.Count} связей, {graph.AllNetworkNodes.Count} узлов");
        }

        // 3. Проверка потоков
        Console.WriteLine($"   • Потоки данных: {scenario.Flows.Count}");
        foreach (var flow in scenario.Flows)
        {
            Console.WriteLine($"     - {flow.Id}: {flow.Volume} ГБ, {flow.Transformations.Count} трансформаций");
        }

        // 4. КРИТИЧЕСКАЯ ПРОВЕРКА: поиск путей ДО синтеза
        Console.WriteLine("\n   🔎 Поиск путей достижимости (без синтеза)...");
        var reachabilityUseCase = _services.GetRequiredService<ICheckReachabilityUseCase>();

        var request = new ReachabilityRequest
        {
            SourceNode = scenario.NodeInputs.Keys.First().NodeId,
            TargetNodes = scenario.OutputNodes.Select(n => n.NodeId).ToList(),
            CustomInterval = scenario.CustomInterval
        };

        try
        {
            var result = reachabilityUseCase.Execute(scenario.Graphs, request);

            Console.WriteLine($"   • Пути найдены: {result.IsReachable}");
            Console.WriteLine($"   • Количество путей: {result.AllPaths?.Count ?? 0}");

            if (result.AllPaths != null && result.AllPaths.Any())
            {
                Console.WriteLine("   • Пример пути:");
                var firstPath = result.AllPaths.First();
                Console.WriteLine($"     {string.Join(" → ", firstPath.Path)}");
                Console.WriteLine($"     Интервал: [{firstPath.Interval.Start}, {firstPath.Interval.End}]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("   ⚠️  КРИТИЧЕСКИЙ СИГНАЛ: пути не найдены!");
                Console.ResetColor();
                Console.WriteLine("      Без путей синтез не может определить требования к ресурсам.");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   ❌ Ошибка поиска путей: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    public async Task RunPostSynthesisDiagnosticsAsync(IReadOnlyList<StructConfiguration> configs, TestScenario scenario)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🔍 ДИАГНОСТИКА ПОСЛЕ СИНТЕЗА");
        Console.ResetColor();

        // 1. Анализ активных функций
        var totalActiveProcesses = configs.Sum(c => c.Nodes.Sum(n => n.ActiveProcesses.Count));
        var totalActiveTransports = configs.Sum(c => c.Links.Sum(l => l.ActiveTransports.Count));
        var totalStorageConfigs = configs.Sum(c => c.Nodes.Sum(n => n.StorageCapacities.Count));

        Console.WriteLine($"   • Активные процессы: {totalActiveProcesses}");
        Console.WriteLine($"   • Активные транспорты: {totalActiveTransports}");
        Console.WriteLine($"   • Конфигурации хранилищ: {totalStorageConfigs}");

        // 2. Поиск "мёртвых" конфигураций
        var deadConfigs = configs.Where(c =>
            !c.Nodes.Any(n => n.ActiveProcesses.Any() || n.StorageCapacities.Any()) &&
            !c.Links.Any(l => l.ActiveTransports.Any())).ToList();

        if (deadConfigs.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   ⚠️  Обнаружено {deadConfigs.Count} 'мёртвых' конфигураций (без активных функций)");
            Console.ResetColor();

            foreach (var config in deadConfigs)
            {
                Console.WriteLine($"     - Интервал [{config.Interval.Start}, {config.Interval.End}]");
            }
        }

        Console.WriteLine();
    }
}