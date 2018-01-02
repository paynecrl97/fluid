namespace Fluid.Tests.Mocks
{
    public class MockFilePathFactory : IFluidFilePathFactory
    {
        public string CreateFilePath(string fileName)
        {
            return $"My\\Custom\\Path\\{fileName}";
        }
    }
}