using DynamicNetwork.Application.Interfaces.Factories;
using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.Services;
using DynamicNetwork.Application.Interfaces.Session;
using DynamicNetwork.Application.Interfaces.UseCases.Analysis;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;
using DynamicNetwork.Application.Interfaces.UseCases.Flows;
using DynamicNetwork.Application.Interfaces.UseCases.Graphs;
using DynamicNetwork.Application.Interfaces.UseCases.Library;
using DynamicNetwork.Application.Interfaces.UseCases.Reachability;
using DynamicNetwork.Application.Session;
using DynamicNetwork.Application.UseCases.Analysis;
using DynamicNetwork.Application.UseCases.Configuration;
using DynamicNetwork.Application.UseCases.Flows;
using DynamicNetwork.Application.UseCases.Graphs;
using DynamicNetwork.Application.UseCases.Library;
using DynamicNetwork.Application.UseCases.Reachability;
using DynamicNetwork.Domain.Functions;
using DynamicNetwork.Domain.Services;
using DynamicNetwork.Infrastructure.Adapters.FileStorage;
using DynamicNetwork.Infrastructure.Adapters.TemporalData;
using DynamicNetwork.Infrastructure.DomainServices;
using DynamicNetwork.Infrastructure.Factories;
using DynamicNetwork.Infrastructure.FileStorage;
using DynamicNetwork.Infrastructure.Persistence.Providers;
using DynamicNetwork.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicNetwork.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicNetworkSynthesis(
          this IServiceCollection services)
    {
        // Доменные сервисы
        services.AddSingleton<ISynthesisDomainService, SynthesisDomainService>();
        services.AddSingleton<IPathFindingDomainService, PathFindingDomainService>();
        services.AddSingleton<IGraphAnalysisDomainService, GraphAnalysisDomainService>();

        // Провайдеры и репозитории
        services.AddSingleton<IFunctionLibraryProvider, InMemoryFunctionLibraryProvider>();
        services.AddScoped<IStructConfigurationRepository, InMemoryStructConfigurationRepository>();
        services.AddScoped<IDataFlowRepository, InMemoryDataFlowRepository>();
        services.AddScoped<IGraphSessionManager, GraphSessionManager>();

        // Фабрики и порты
        services.AddScoped<ITemporalGraphFactory, TemporalGraphFactory>();
        services.AddScoped<ITemporalDataSourcePort, JsonTemporalDataSourceAdapter>();
        services.AddScoped<IFileStoragePort, XmlFileStorageAdapter>();
        services.AddScoped<IFunctionLibraryFilePort, XmlFunctionLibraryFileAdapter>();
        services.AddScoped<IConfigurationExportService, XmlConfigurationExportAdapter>();

        // Сценарии использования
        services.AddScoped<IManageFunctionLibraryUseCase, ManageFunctionalLibraryUseCase>();
        services.AddScoped<IImportFunctionLibraryUseCase, ImportFunctionLibraryUseCase>();
        services.AddScoped<IExportFunctionLibraryUseCase, ExportFunctionLibraryUseCase>();
        services.AddScoped<IManageStructConfigurationUseCase, ManageStructConfigurationUseCase>();
        services.AddScoped<IEditStructConfigurationUseCase, EditStructConfigurationUseCase>();
        services.AddScoped<IManageDataFlowsUseCase, ManageDataFlowsUseCase>();
        services.AddScoped<ILoadTemporalGraphsUseCase, LoadTemporalGraphsUseCase>();
        services.AddScoped<ICheckReachabilityUseCase, CheckReachabilityUseCase>();
        services.AddScoped<IAnalyzeGraphStructureUseCase, AnalyzeGraphStructureUseCase>();
        services.AddScoped<ISynthesizeConfigurationUseCase, SynthesizeConfigurationUseCase>();
        services.AddScoped<IExportConfigurationUseCase, ExportConfigurationUseCase>();

        return services;
    }
}
