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
            var iEncryptionProviderProviderName = typeof(IEncryptionProviderProvider).AssemblyQualifiedName;

            if (iEncryptionProviderProviderName == null)
            {
                return;
            }

            IEnumerable<string> files;

            try
            {
                files =
                    Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                    .Concat(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"));
            }
            catch
            {
                return;
            }

            try
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AppDomainOnReflectionOnlyAssemblyResolve;

                var encryptionProviderTypes = new List<string>();

                foreach (var file in files)
                {
                    try
                    {
                        var assembly = Assembly.ReflectionOnlyLoadFrom(file);

                        if (assembly.FullName == typeof(ModuleInitializer).Assembly.FullName)
                        {
                            continue;
                        }

                        encryptionProviderTypes.AddRange(
                            assembly.GetTypes()
                                .Where(t =>
                                    t.IsClass
                                    && !t.IsAbstract
                                    && t.IsPublic
                                    && t.AssemblyQualifiedName != null
                                    && t.GetInterfaces().Any(i => i.AssemblyQualifiedName == iEncryptionProviderProviderName)
                                    && t.GetConstructor(Type.EmptyTypes) != null)
                                .Select(t => t.AssemblyQualifiedName));
                    }
                    catch
                    {
                    }
                }

                if (encryptionProviderTypes.Count == 1)
                {
                    IEncryptionProviderProvider encryptionProviderProvider = null;

                    var encryptionProviderType = Type.GetType(encryptionProviderTypes[0]);

                    if (encryptionProviderType != null)
                    {
                        encryptionProviderProvider = (IEncryptionProviderProvider)Activator.CreateInstance(encryptionProviderType);
                    }

                    if (encryptionProviderProvider != null)
                    {
                        EncryptionProvider.Current = encryptionProviderProvider.GetEncryptionProvider();
                    }
                }
            }
            catch
            {
            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= AppDomainOnReflectionOnlyAssemblyResolve;
            }
        }

        private static Assembly AppDomainOnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}