using System;

namespace XSerializer.Encryption
{
    /// <summary>
    /// Indicates that a class should be used to set the value of
    /// <see cref="EncryptionMechanism.Current"/>. In order to be used,
    /// a non-abstract class decorated by this attribute (or, if this attribute 
    /// decorates an assembly, the type indicated by the <see cref="ClassType"/>
    /// property) must implement either <see cref="IEncryptionMechanism"/> or 
    /// <see cref="IEncryptionMechanismFactory"/> and provide either a public 
    /// parameterless constructor or a constructor whose parameters are all
    /// optional (have a default value).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EncryptionMechanismAttribute : Attribute
    {
        private readonly Type _classType;
        private readonly int _priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class.
        /// This constructor should not be used when this attribute decorates an assembly.
        /// </summary>
        public EncryptionMechanismAttribute()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class.
        /// This constructor should not be used when this attribute decorates an assembly.
        /// </summary>
        /// <param name="priority">The priority of the class.</param>
        public EncryptionMechanismAttribute(int priority)
            : this(null, priority)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class.
        /// </summary>
        /// <param name="classType">
        /// The type of the class to export. If this attribute decorates a class, then
        /// this value will be ignored.
        /// </param>
        public EncryptionMechanismAttribute(Type classType)
            : this(classType, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionMechanismAttribute"/> class.
        /// </summary>
        /// <param name="classType">
        /// The type of the class to export. If this attribute decorates a class, then
        /// this value will be ignored.
        /// </param>
        /// <param name="priority">The priority of the class.</param>
        public EncryptionMechanismAttribute(Type classType, int priority)
        {
            _priority = priority;
            _classType = classType;
        }

        /// <summary>
        /// Gets the type of class to export. If this attribute decorates a class
        /// (as opposed to an assembly), this value is ignored.
        /// </summary>
        public Type ClassType { get { return _classType; } }

        /// <summary>
        /// Gets a value that indicates this class's relative priority.
        /// </summary>
        public int Priority { get { return _priority; } }

        /// <summary>
        /// Gets a value indicating whether this class is explicitly ineligible 
        /// for to be used as the encryption mechanism for the current app domain.
        /// </summary>
        public bool Disabled { get; set; }
    }
}