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

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class
        /// with its <see cref="Priority"/> property set to zero.
        /// </summary>
        public EncryptionMechanismAttribute()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class
        /// with its <see cref="Priority"/> property set to the value of the 
        /// <paramref name="priority"/> parameter.
        /// </summary>
        /// <param name="priority">A value that indicates the priority of a class. This value is used
        /// when multiple implementations of <see cref="IEncryptionMechanism"/> or
        /// <see cref="IEncryptionMechanismFactory"/> are discovered. The one with
        /// the highest priority will be used.</param>
        public EncryptionMechanismAttribute(int priority)
        {
            _priority = priority;
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
    }
}