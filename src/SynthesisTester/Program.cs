using DynamicNetwork.Infrastructure.DependencyInjection;
using DynamicNetwork.SynthesisTester;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace DynamicNetwork.SynthesisTester;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  DynamicNetwork — Тестер синтеза (с полным DI)                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Создаём хост с DI-контейнером
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // ← РЕГИСТРИРУЕМ ВСЮ СИСТЕМУ КАК В ОСНОВНОМ ПРИЛОЖЕНИИ
                services.AddDynamicNetworkSynthesis();

                // Добавляем сервисы тестера
                services.AddSingleton<TestScenarios>();
                services.AddSingleton<Diagnostics>();
                services.AddSingleton<TesterHost>();
            })
            .Build();

        try
        {
            // Запускаем хост
            await host.StartAsync();

            // Получаем тестер из контейнера
            var tester = host.Services.GetRequiredService<TesterHost>();

            // Запускаем интерактивный режим
            await tester.RunInteractiveAsync();

            // Останавливаем хост
            await host.StopAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            // Dispose хоста
            if (host is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (host is IDisposable disposable)
                disposable.Dispose();
        }
    }
}