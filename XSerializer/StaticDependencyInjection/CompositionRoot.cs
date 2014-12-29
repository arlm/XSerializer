using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XSerializer.Encryption;

namespace XSerializer.StaticDependencyInjection
{
    internal partial class CompositionRoot
    {
        public override void Bootstrap()
        {
            ImportSingle<IEncryptionMechanism, IEncryptionMechanismFactory>(
                mechanism => EncryptionMechanism.Current = mechanism,
                factory => factory.GetEncryptionMechanism());
        }

        protected override ExportInfo GetExportInfo(Type type)
        {
            var attribute = Attribute.GetCustomAttribute(type, typeof(EncryptionMechanismAttribute)) as EncryptionMechanismAttribute;

            if (attribute == null)
            {
                return base.GetExportInfo(type);
            }

            return
                new ExportInfo(type, attribute.Priority)
                {
                    Disabled = attribute.Disabled
                };
        }

        protected override IEnumerable<ExportInfo> GetExportInfos(
            IEnumerable<CustomAttributeData> assemblyAttributeDataCollection)
        {
            return
                assemblyAttributeDataCollection.AsAttributeType<EncryptionMechanismAttribute>()
                    .Where(attribute => attribute.ClassType.IsClass)
                    .Select(attribute =>
                        new ExportInfo(attribute.ClassType, attribute.Priority)
                        {
                            Disabled = attribute.Disabled
                        });
        }
    }
}
