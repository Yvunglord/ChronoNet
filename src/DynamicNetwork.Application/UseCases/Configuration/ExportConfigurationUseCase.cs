using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.Services;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;

namespace DynamicNetwork.Application.UseCases.Configuration;

public class ExportConfigurationUseCase : IExportConfigurationUseCase
{
    private readonly IConfigurationExportService _exportService;
    private readonly IStructConfigurationRepository _configRepo;
    private readonly IDataFlowRepository _flowRepo;
    private readonly IFunctionLibraryProvider _libraryProvider;
    private readonly IFileStoragePort _fileStorage;

    public ExportConfigurationUseCase(
        IConfigurationExportService exportService,
        IStructConfigurationRepository configRepo,
        IDataFlowRepository flowRepo,
        IFunctionLibraryProvider libraryProvider,
        IFileStoragePort fileStorage)
    {
        _exportService = exportService;
        _configRepo = configRepo;
        _flowRepo = flowRepo;
        _libraryProvider = libraryProvider;
        _fileStorage = fileStorage;
    }

    public void Execute(string outputPath)
    {
        var configs = _configRepo.GetAll();
        if (!configs.Any())
            throw new InvalidOperationException("No configurations to export");

        var library = _libraryProvider.GetCurrent();
        var flows = _flowRepo.GetAll();

        var document = _exportService.Export(configs, library, flows);

        _fileStorage.SaveXml(document, outputPath);
    }
}
