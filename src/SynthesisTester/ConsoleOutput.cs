namespace DynamicNetwork.SynthesisTester;

public static class ConsoleOutput
{
    public static void PrintSynthesisResult(SynthesisResult result)
    {
        PrintHeader("РЕЗУЛЬТАТЫ СИНТЕЗА");

        PrintSection("📊 Общая информация");
        PrintKeyValue("Временной интервал", $"[{result.TimeInterval.Start}, {result.TimeInterval.End}]");
        PrintKeyValue("Количество потоков", result.FlowCount.ToString());
        PrintKeyValue("Количество конфигураций", result.Configurations.Count.ToString());
        PrintKeyValue("Входные узлы", string.Join(", ", result.InputNodes.Select(n => n.NodeId)));
        PrintKeyValue("Выходные узлы", string.Join(", ", result.OutputNodes.Select(n => n.NodeId)));

        PrintSection("⚙️  Синтезированные конфигурации");
        foreach (var config in result.Configurations)
        {
            PrintSubsection($"Интервал [{config.Interval.Start}, {config.Interval.End}]");

            Console.WriteLine("  Узлы:");
            foreach (var node in config.Nodes)
            {
                var active = node.ActiveProcesses.Count > 0
                    ? $" (активные: {string.Join(", ", node.ActiveProcesses)})"
                    : " (без активных процессов)";
                Console.WriteLine($"    • {node.NodeId}{active}");

                if (node.StorageCapacities.Count > 0)
                {
                    foreach (var storage in node.StorageCapacities)
                    {
                        Console.WriteLine($"      └─ Хранилище {storage.Key}: {storage.Value} ГБ");
                    }
                }
            }

            Console.WriteLine("\n  Связи:");
            foreach (var link in config.Links)
            {
                var active = link.ActiveTransports.Count > 0
                    ? $" (активные: {string.Join(", ", link.ActiveTransports)})"
                    : " (без активных транспортов)";
                Console.WriteLine($"    • {link.NodeA} ⟷ {link.NodeB}{active}");
            }
            Console.WriteLine();
        }

        PrintSection("✅ Вывод");
        if (result.Configurations.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Синтез успешно завершён. Созданы конфигурации для всех временных интервалов.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠️  Синтез завершён, но не создано ни одной конфигурации.");
            Console.ResetColor();
        }
    }

    private static void PrintHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 70));
        Console.WriteLine($"  {text}");
        Console.WriteLine(new string('═', 70));
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void PrintSection(string text)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"▸ {text}");
        Console.ResetColor();
    }

    private static void PrintSubsection(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ▹ {text}");
        Console.ResetColor();
    }

    private static void PrintKeyValue(string key, string value)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  {key}: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(value);
        Console.ResetColor();
    }
}