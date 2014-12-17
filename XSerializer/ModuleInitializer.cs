using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XSerializer.Encryption;

namespace XSerializer
{
    public static class ModuleInitializer // Future devs: Do not change the name of this class
    {
        public static void Run() // Future devs: Do not change the signature of this method
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AppDomainOnReflectionOnlyAssemblyResolve;

            var iEncryptionProviderAssemblyQualifiedName = typeof(IEncryptionProvider).AssemblyQualifiedName;

            var files =
                Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Concat(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"));

            var encryptionProviders = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(file);

                    if (assembly.FullName == typeof(ModuleInitializer).Assembly.FullName)
                    {
                        continue;
                    }

                    encryptionProviders.AddRange(
                        assembly.GetTypes()
                            .Where(t =>
                                t.IsClass
                                && !t.IsAbstract
                                && t.IsPublic
                                && t.GetInterfaces().Any(i => i.AssemblyQualifiedName == iEncryptionProviderAssemblyQualifiedName)
                                && t.GetConstructor(Type.EmptyTypes) != null
                                && t.AssemblyQualifiedName != null)
                            .Select(t => t.AssemblyQualifiedName));
                }
                catch
                {
                }
            }

            if (encryptionProviders.Count == 1)
            {
                IEncryptionProvider encryptionProvider = null;

                try
                {
                    var encryptionProviderType = Type.GetType(encryptionProviders[0]);

                    if (encryptionProviderType != null)
                    {
                        encryptionProvider = (IEncryptionProvider)Activator.CreateInstance(encryptionProviderType);
                    }
                }
                catch
                {
                }

                if (encryptionProvider != null)
                {
                    EncryptionProvider.Current = encryptionProvider;
                }
            }

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= AppDomainOnReflectionOnlyAssemblyResolve;
        }

        private static Assembly AppDomainOnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}