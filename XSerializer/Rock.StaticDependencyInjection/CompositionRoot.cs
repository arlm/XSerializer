using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using XSerializer.Encryption;

namespace XSerializer.Rock.StaticDependencyInjection
{
    internal partial class CompositionRoot : CompositionRootBase
    {
        public override void Bootstrap()
        {
            ImportSingle<IEncryptionMechanism, IEncryptionMechanismFactory>(
                mechanism => EncryptionMechanism.Current = mechanism,
                factory => factory.GetEncryptionMechanism());
        }

        /// <summary>
        /// Gets a value indicating whether static dependency injection is enabled.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                const string key = "XSerializer.StaticDependencyInjection.Enabled";
                var enabledValue = ConfigurationManager.AppSettings.Get(key) ?? "true";
                return enabledValue.ToLower() != "false";
            }
        }

        /// <summary>
        /// Return a collection of metadata objects that describe the export operations for a type.
        /// </summary>
        /// <param name="type">The type to get export metadata.</param>
        /// <returns>A collection of metadata objects that describe export operations.</returns>
        protected override IEnumerable<ExportInfo> GetExportInfos(Type type)
        {
            var attributes = Attribute.GetCustomAttributes(type, typeof(EncryptionMechanismAttribute));

            if (attributes.Length == 0)
            {
                return base.GetExportInfos(type);
            }

            return
                attributes.Cast<EncryptionMechanismAttribute>()
                .Select(attribute =>
                    new ExportInfo(type, attribute.Priority)
                    {
                        Disabled = attribute.Disabled
                    });
        }
    }
}
