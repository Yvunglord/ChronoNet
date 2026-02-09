using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicNetwork.SynthesisTester;

public class TesterHost
{
    private readonly IServiceProvider _services;
    private readonly TestScenarios _scenarios;
    private readonly Diagnostics _diagnostics;
    private readonly IFunctionLibraryProvider _libraryProvider;
    private readonly IStructConfigurationRepository _configRepo;
    private readonly IDataFlowRepository _flowRepo;

    public TesterHost(
        IServiceProvider services,
        TestScenarios scenarios,
        Diagnostics diagnostics,
        IFunctionLibraryProvider libraryProvider,
        IStructConfigurationRepository configRepo,
        IDataFlowRepository flowRepo)
    {
        _services = services;
        _scenarios = scenarios;
        _diagnostics = diagnostics;
        _libraryProvider = libraryProvider;
        _configRepo = configRepo;
        _flowRepo = flowRepo;
    }

    public async Task RunInteractiveAsync()
    {
        while (true)
        {
            PrintMenu();

            var input = Console.ReadLine();
            if (input == "0") break;

            if (int.TryParse(input, out var choice) && choice >= 1 && choice <= 6)
            {
                await RunScenarioAsync(choice);
            }
            else
            {
                Console.WriteLine("\n⚠️  Неверный выбор. Пожалуйста, введите число от 0 до 5.");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("\nДоступные тестовые сценарии:");
        Console.WriteLine("  1. Простой видео-поток (4K → 1080p → H.264)");
        Console.WriteLine("  2. Два параллельных потока (видео + аудио)");
        Console.WriteLine("  3. Сеть с изолированными узлами");
        Console.WriteLine("  4. Минимальная сеть (2 узла, 1 связь)");
        Console.WriteLine("  5. Сложная сеть с множественными путями");
        Console.WriteLine("  6. Тест из файла");
        Console.WriteLine("  0. Выход");
        Console.Write("\nВыберите сценарий (0-6): ");
    }

    private async Task RunScenarioAsync(int scenarioId)
    {
        Console.WriteLine("\n" + new string('─', 70));
        Console.WriteLine($"Запуск сценария #{scenarioId}: {_scenarios.GetScenarioName(scenarioId)}");
        Console.WriteLine(new string('─', 70) + "\n");

        try
        {
            // 1. Получаем сценарий
            var scenario = _scenarios.GetScenario(scenarioId);

            // 2. Диагностика ДО синтеза
            await _diagnostics.RunPreSynthesisDiagnosticsAsync(scenario);

            // 3. Инициализация состояния системы
            InitializeSystemState(scenario);

            // 4. Запуск синтеза через правильно сконфигурированный сценарий
            var synthesizeUseCase = _services.GetRequiredService<ISynthesizeConfigurationUseCase>();

            var request = new StructConfigurationRequestDto
            {
                NodeInputs = scenario.NodeInputs,
                OutputNodes = scenario.OutputNodes,
                CustomInterval = scenario.CustomInterval
            };

            Console.WriteLine("⚙️  Запуск синтеза через ISynthesizeConfigurationUseCase.Execute()...");
            var configs = synthesizeUseCase.Execute(request, scenario.Graphs, scenario.Flows);

            var exportUseCase = _services.GetRequiredService<IExportConfigurationUseCase>();

            exportUseCase.Execute("C:\\Users\\k_yak\\Desktop\\pupupu.xml");

            Console.WriteLine($"✅ Синтез завершён. Создано конфигураций: {configs.Count}\n");

            // 5. Диагностика ПОСЛЕ синтеза
            await _diagnostics.RunPostSynthesisDiagnosticsAsync(configs, scenario);

            // 6. Вывод результатов
            PrintSynthesisResults(configs, scenario);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Ошибка синтеза: {ex.Message}");
            Console.ResetColor();

            if (Environment.GetEnvironmentVariable("DEBUG") == "1")
            {
                Console.WriteLine("\nДетали стека:");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    private void InitializeSystemState(TestScenario scenario)
    {
        Console.WriteLine("🔧 Инициализация состояния системы...");

        // Очистка репозиториев
        foreach (var config in _configRepo.GetAll().ToList())
            _configRepo.Delete(config.Interval);

        foreach (var flow in _flowRepo.GetAll().ToList())
            _flowRepo.Delete(flow.Id);

        // Инициализация библиотеки функций
        _libraryProvider.Update(scenario.Library);
        Console.WriteLine($"   • Библиотека функций: {scenario.Library.Processes.Count} процессов, " +
                          $"{scenario.Library.Transports.Count} транспортов, " +
                          $"{scenario.Library.Storages.Count} хранилищ");

        // Сохранение потоков данных
        foreach (var flow in scenario.Flows)
        {
            _flowRepo.Add(flow);
        }
        Console.WriteLine($"   • Потоки данных: {scenario.Flows.Count} потоков");

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

        foreach (var c in configs)
            _configRepo.Add(c);
        Console.WriteLine($"   • Базовые конфигурации: {scenario.Graphs.Count} интервалов");
        Console.WriteLine();
    }

    private void PrintSynthesisResults(IReadOnlyList<StructConfiguration> configs, TestScenario scenario)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 70));
        Console.WriteLine("РЕЗУЛЬТАТЫ СИНТЕЗА");
        Console.WriteLine(new string('═', 70));
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("📊 Общая информация:");
        Console.WriteLine($"   • Временной интервал: [{scenario.CustomInterval.Start}, {scenario.CustomInterval.End}]");
        Console.WriteLine($"   • Количество потоков: {scenario.Flows.Count}");
        Console.WriteLine($"   • Количество конфигураций: {configs.Count}");
        Console.WriteLine($"   • Входные узлы: {string.Join(", ", scenario.NodeInputs.Keys.Select(n => n.NodeId))}");
        Console.WriteLine($"   • Выходные узлы: {string.Join(", ", scenario.OutputNodes.Select(n => n.NodeId))}");
        Console.WriteLine();

        Console.WriteLine("⚙️  Синтезированные конфигурации:");
        foreach (var config in configs)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   ▹ Интервал [{config.Interval.Start}, {config.Interval.End}]");
            Console.ResetColor();

            Console.WriteLine("     Узлы:");
            foreach (var node in config.Nodes)
            {
                var activeCount = node.ActiveProcesses.Count;
                var storageCount = node.StorageCapacities.Count;

                if (activeCount > 0 || storageCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"       • {node.NodeId}");

                    if (activeCount > 0)
                        Console.Write($" [процессы: {string.Join(", ", node.ActiveProcesses)}]");

                    if (storageCount > 0)
                        Console.Write($" [хранилища: {storageCount}]");

                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"       • {node.NodeId} ⚠️  БЕЗ АКТИВНЫХ ФУНКЦИЙ");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n     Связи:");
            foreach (var link in config.Links)
            {
                var activeCount = link.ActiveTransports.Count;

                if (activeCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"       • {link.NodeA} ⟷ {link.NodeB} [транспорты: {string.Join(", ", link.ActiveTransports)}]");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"       • {link.NodeA} ⟷ {link.NodeB} ⚠️  БЕЗ АКТИВНЫХ ТРАНСПОРТОВ");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }

        // Итоговая диагностика
        var hasActiveFunctions = configs.Any(c =>
            c.Nodes.Any(n => n.ActiveProcesses.Any() || n.StorageCapacities.Any()) ||
            c.Links.Any(l => l.ActiveTransports.Any()));

        Console.WriteLine(new string('─', 70));
        if (hasActiveFunctions)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ СИНТЕЗ УСПЕШЕН: в конфигурациях активированы функции");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ СИНТЕЗ ПРОВАЛЕН: все конфигурации пустые (нет активных функций)");
            Console.WriteLine("\nВозможные причины:");
            Console.WriteLine("  1. Алгоритм поиска путей не находит маршруты");
            Console.WriteLine("  2. Агрегация требований не формирует запросы к ресурсам");
            Console.WriteLine("  3. Синтез не подбирает функции из библиотеки");
            Console.WriteLine("  4. Ошибка в логике активации функций");
        }
        Console.ResetColor();
        Console.WriteLine(new string('─', 70));
    }
}