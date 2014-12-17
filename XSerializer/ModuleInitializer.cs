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
            var iEncryptionProviderName = typeof(IEncryptionProvider).AssemblyQualifiedName;

            if (iEncryptionProviderName == null)
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
                                    && t.GetInterfaces().Any(i => i.AssemblyQualifiedName == iEncryptionProviderName)
                                    && t.GetConstructor(Type.EmptyTypes) != null)
                                .Select(t => t.AssemblyQualifiedName));
                    }
                    catch
                    {
                    }
                }

                if (encryptionProviderTypes.Count == 1)
                {
                    IEncryptionProvider encryptionProvider = null;

                    var encryptionProviderType = Type.GetType(encryptionProviderTypes[0]);

                    if (encryptionProviderType != null)
                    {
                        encryptionProvider = (IEncryptionProvider)Activator.CreateInstance(encryptionProviderType);
                    }

                    if (encryptionProvider != null)
                    {
                        EncryptionProvider.Current = encryptionProvider;
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