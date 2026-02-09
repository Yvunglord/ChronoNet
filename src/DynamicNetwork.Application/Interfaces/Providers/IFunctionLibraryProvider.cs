using DynamicNetwork.Domain.Functions;

namespace DynamicNetwork.Application.Interfaces.Providers;

public interface IFunctionLibraryProvider
{
    FunctionLibrary GetCurrent();
    void Update(FunctionLibrary library);
}
