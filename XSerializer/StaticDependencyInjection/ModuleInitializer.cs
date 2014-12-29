namespace XSerializer.StaticDependencyInjection
{
    internal static class ModuleInitializer
    {
        internal static void Run()
        {
            new CompositionRoot().Bootstrap();
        }
    }
}
