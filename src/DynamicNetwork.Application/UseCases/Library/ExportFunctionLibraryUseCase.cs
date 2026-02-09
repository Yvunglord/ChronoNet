using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.UseCases.Library;

namespace DynamicNetwork.Application.UseCases.Library;

public class ExportFunctionLibraryUseCase : IExportFunctionLibraryUseCase
{
    private readonly IFunctionLibraryProvider _provider;
    private readonly IFunctionLibraryFilePort _filePort;

    public ExportFunctionLibraryUseCase(
        IFunctionLibraryProvider provider,
        IFunctionLibraryFilePort filePort)
    {
        _provider = provider;
        _filePort = filePort;
    }

    public void Execute(string filePath)
    {
        var library = _provider.GetCurrent();

        _filePort.Save(library, filePath);
    }
}
