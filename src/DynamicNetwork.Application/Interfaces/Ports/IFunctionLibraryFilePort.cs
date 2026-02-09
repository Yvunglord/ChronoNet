using DynamicNetwork.Domain.Functions;

namespace DynamicNetwork.Application.Interfaces.Ports;

public interface IFunctionLibraryFilePort
{
    FunctionLibrary Load(string path);
    void Save(FunctionLibrary library, string path);
}
