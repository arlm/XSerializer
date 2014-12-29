using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using XSerializer.Encryption;

namespace XSerializer
{
    public static class ModuleInitializer // Future devs: Do not change the name of this class
    {
        private const string _eventLogSource = ".NET Runtime";
        private const string _eventLogName = "Application";

        private static readonly string _iEncryptionMechanismName = typeof(IEncryptionMechanism).AssemblyQualifiedName;
        private static readonly string _iEncryptionMechanismFactoryName = typeof(IEncryptionMechanismFactory).AssemblyQualifiedName;

        public static void Run() // Future devs: Do not change the signature of this method
        {
            CheckEventLogSource();
            SetCurrentEncryptionMechanism();
        }

        private static void CheckEventLogSource()
        {
            try
            {
                if (!EventLog.SourceExists(_eventLogSource))
                {
                    EventLog.CreateEventSource(_eventLogSource, _eventLogName);
                }
            }
            catch
            {
            }
        }

        private static void SetCurrentEncryptionMechanism()
        {
            if (_iEncryptionMechanismName == null || _iEncryptionMechanismFactoryName == null)
            {
                return;
            }

            try
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AppDomainOnReflectionOnlyAssemblyResolve;

                var prioritizedGroupsOfCandidateTypes =
                    GetAssemblyFiles()
                        .SelectMany(LoadCandidateTypes)
                        .GroupBy(x => x.Priority, item => item.Type)
                        .OrderByDescending(g => g.Key);

                foreach (var candidateTypes in prioritizedGroupsOfCandidateTypes.Select(g => g.ToList()))
                {
                    var candidateType = ChooseCandidateType(candidateTypes);

                    if (candidateType == null)
                    {
                        WriteEventLogWarning(candidateTypes);
                        continue;
                    }

                    var encryptionMechanism = GetEncryptionMechanism(candidateType);

                    if (encryptionMechanism != null)
                    {
                        EncryptionMechanism.Current = encryptionMechanism;
                        return;
                    }
                }
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

        private static IEnumerable<string> GetAssemblyFiles()
        {
            try
            {
                return
                    Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                        .Concat(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"));
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<PrioritizedType> LoadCandidateTypes(string assemblyFile)
        {
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

                if (assembly.FullName == typeof(ModuleInitializer).Assembly.FullName)
                {
                    return Enumerable.Empty<PrioritizedType>();
                }

                return
                    assembly.GetTypes()
                        .Where(t =>
                            t.IsClass
                            && !t.IsAbstract
                            && t.IsPublic
                            && t.AssemblyQualifiedName != null
                            && t.GetInterfaces().Any(i =>
                                i.AssemblyQualifiedName == _iEncryptionMechanismFactoryName
                                || i.AssemblyQualifiedName == _iEncryptionMechanismName)
                            && t.HasDefaultishConstructor())
                        .Select(t => GetPrioritizedType(t.AssemblyQualifiedName))
                        .Where(x => x != null);
            }
            catch
            {
                return Enumerable.Empty<PrioritizedType>();
            }
        }

        private static bool HasDefaultishConstructor(this Type type)
        {
            return
                type.GetConstructor(Type.EmptyTypes) != null
                || type.GetConstructors().Any(ctor => ctor.GetParameters().All(HasDefaultValue));
        }

        private static bool HasDefaultValue(ParameterInfo parameter)
        {
            const ParameterAttributes hasDefaultValue =
                ParameterAttributes.HasDefault | ParameterAttributes.Optional;

            return (parameter.Attributes & hasDefaultValue) == hasDefaultValue;
        }

        private static PrioritizedType GetPrioritizedType(string assemblyQualifiedName)
        {
            try
            {
                var type = Type.GetType(assemblyQualifiedName);

                if (type == null)
                {
                    return null;
                }

                var encryptionMechanismAttribute =
                    (EncryptionMechanismAttribute)Attribute.GetCustomAttribute(type, typeof(EncryptionMechanismAttribute));

                var priority =
                    encryptionMechanismAttribute != null
                        ? encryptionMechanismAttribute.Priority
                        : -1;

                return new PrioritizedType
                {
                    Type = type,
                    Priority = priority
                };
            }
            catch
            {
                return null;
            }
        }

        private static Type ChooseCandidateType(IList<Type> candidateTypes)
        {
            if (candidateTypes.Count == 1)
            {
                return candidateTypes[0];
            }

            // Check for the case where only one type implements IEncryptionMechanismFactory,
            // and all others only implement IEncryptionMechanism. In this case, because
            // IEncryptionMechanismFactory has higher priority, use that single type.
            var data =
                candidateTypes.Select(type =>
                {
                    var interfaces = type.GetInterfaces();

                    return new
                    {
                        Type = type,
                        IsMechanism = interfaces.Any(i => i.AssemblyQualifiedName == _iEncryptionMechanismName),
                        IsMechanismFactory = interfaces.Any(i => i.AssemblyQualifiedName == _iEncryptionMechanismFactoryName)
                    };
                }).ToArray();

            if ((data.Count(x => x.IsMechanismFactory) == 1)
                && (data.Count(x => x.IsMechanism && !x.IsMechanismFactory) == (data.Length - 1)))
            {
                return data.Single(x => x.IsMechanismFactory).Type;
            }

            return null;
        }

        private static void WriteEventLogWarning(IEnumerable<Type> duplicatePriorityTypes)
        {
            try
            {
                var eventLog = new EventLog(_eventLogName)
                {
                    Source = _eventLogSource,
                    MachineName = Environment.MachineName
                };

                const string separator = "\r\n        ";

                var message =
                    string.Format(
                        "XSerializer module initialization warning:\r\n    More than one implementation of either IEncryptionMechanism or IEncryptionMechanismFactory was discovered with the same priority.\r\n    The types were:{0}{1}",
                        separator,
                        string.Join(separator, duplicatePriorityTypes.Select(t => t.FullName)));

                eventLog.WriteEntry(message, EventLogEntryType.Warning);
            }
            catch
            {
            }
        }

        private static IEncryptionMechanism GetEncryptionMechanism(Type candidateType)
        {
            try
            {
                object instance;

                if (candidateType.GetConstructor(Type.EmptyTypes) != null)
                {
                    instance = Activator.CreateInstance(candidateType);
                }
                else
                {
                    var ctor =
                        candidateType.GetConstructors()
                            .OrderByDescending(c => c.GetParameters().Length)
                            .First(c => c.GetParameters().All(HasDefaultValue));

                    var args = ctor.GetParameters().Select(p => p.DefaultValue).ToArray();

                    instance = Activator.CreateInstance(candidateType, args);
                }

                var encryptionMechanismFactory = instance as IEncryptionMechanismFactory;
                if (encryptionMechanismFactory != null)
                {
                    return encryptionMechanismFactory.GetEncryptionMechanism();
                }

                var encryptionMechanism = instance as IEncryptionMechanism;
                if (encryptionMechanism != null)
                {
                    return encryptionMechanism;
                }

                // How did we even get here? Answer me that, Mr. Compiler!
                return null;
            }
            catch
            {
                return null;
            }
        }

        private class PrioritizedType
        {
            public Type Type { get; set; }
            public int Priority { get; set; }
        }
    }
}