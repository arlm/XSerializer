using System;

namespace XSerializer.Encryption
{
    /// <summary>
    /// Indicates that a class should be used to set the value of
    /// <see cref="EncryptionMechanism.Current"/>. In order to be used,
    /// a non-abstract class decorated by this attribute must implement either
    /// <see cref="IEncryptionMechanism"/> or <see cref="IEncryptionMechanismFactory"/>
    /// and provide either a public parameterless constructor or a constructor
    /// whose parameters are all optional (have a default value).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EncryptionMechanismAttribute : Attribute
    {
        private readonly int _priority;
        private readonly bool _disabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class
        /// with its <see cref="Priority"/> property set to zero and its <see cref="Disabled"/>
        /// property set to false.
        /// </summary>
        public EncryptionMechanismAttribute()
            : this(0, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class
        /// with its <see cref="Priority"/> property set to the value of the 
        /// <paramref name="priority"/> parameter and its <see cref="Disabled"/> property set
        /// to false.
        /// </summary>
        /// <param name="priority">A value that indicates the priority of a class. This value is used
        /// when multiple implementations of <see cref="IEncryptionMechanism"/> or
        /// <see cref="IEncryptionMechanismFactory"/> are discovered. The one with
        /// the highest priority will be used.</param>
        public EncryptionMechanismAttribute(int priority)
            : this(priority, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class
        /// with its <see cref="Priority"/> property set to the value of the 
        /// <paramref name="priority"/> parameter and its <see cref="Disabled"/> property set
        /// to the value of the <paramref name="disabled"/> parameter.
        /// </summary>
        /// <param name="priority">A value that indicates the priority of a class. This value is used
        /// when multiple implementations of <see cref="IEncryptionMechanism"/> or
        /// <see cref="IEncryptionMechanismFactory"/> are discovered. The one with
        /// the highest priority will be used.</param>
        /// <param name="disabled">
        /// Whether the class that this attribute decorates should excluded from consideration for
        /// an import operation.
        /// </param>
        public EncryptionMechanismAttribute(int priority, bool disabled)
        {
            _priority = priority;
            _disabled = disabled;
        }

        /// <summary>
        /// Gets a value that indicates the priority of a class. This value is used
        /// when multiple implementations of <see cref="IEncryptionMechanism"/> or
        /// <see cref="IEncryptionMechanismFactory"/> are discovered. The one with
        /// the highest priority will be used.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
        }

        /// <summary>
        /// Gets a value indicating whether the class that this attribute decorates
        /// should excluded from consideration for an import operation.
        /// </summary>
        public bool Disabled
        {
            get { return _disabled; }
        }
    }
}