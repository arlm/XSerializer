using System;

namespace XSerializer
{
    internal class Toggler<T>
    {
        private readonly Action<T> _setValue;
        private readonly T _originalValue;
        private readonly T _newValue;

        private bool _wasToggled;

        public Toggler(Action<T> setValue, T originalValue, T newValue)
        {
            _setValue = setValue;
            _originalValue = originalValue;
            _newValue = newValue;
        }

        /// <summary>
        /// Set the value if the new value is not equal to the original value.
        /// </summary>
        public void Toggle()
        {
            var bothNull =
                ReferenceEquals(_originalValue, null)
                && ReferenceEquals(_newValue, null);

            var equal =
                !ReferenceEquals(_originalValue, null)
                && !ReferenceEquals(_newValue, null)
                && _originalValue.Equals(_newValue);

            if (bothNull || equal)
            {
                return;
            }

            _setValue(_newValue);
            _wasToggled = true;
        }

        /// <summary>
        /// Revert the value if it was previously toggled.
        /// </summary>
        public void Revert()
        {
            if (_wasToggled)
            {
                _setValue(_originalValue);
            }
        }
    }

    /// <summary>
    /// A <see cref="Toggler{T}"/> that sets <see cref="JsonWriter.EncryptWrites"/> to true
    /// when <see cref="Toggler{T}.Toggle"/> is called.
    /// </summary>
    internal class EncryptWritesToggler : Toggler<bool>
    {
        /// <summary>
        /// Creates an instance of <see cref="Toggler{T}"/> that sets the <see cref="JsonWriter.EncryptWrites"/>
        /// property of a <see cref="JsonWriter"/> to true when <see cref="Toggler{T}.Toggle"/>
        /// is called.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> whose <see cref="JsonWriter.EncryptWrites"/>
        /// property is toggled.</param>
        public EncryptWritesToggler(JsonWriter writer)
            : base(x => writer.EncryptWrites = x, writer.EncryptWrites, true)
        {
        }
    }

    /// <summary>
    /// A <see cref="Toggler{T}"/> that sets <see cref="JsonWriter.EncryptWrites"/> to true
    /// when <see cref="Toggler{T}.Toggle"/> is called.
    /// </summary>
    internal class DecryptReadsToggler : Toggler<bool>
    {
        /// <summary>
        /// Creates an instance of <see cref="Toggler{T}"/> that sets the <see cref="JsonReader.DecryptReads"/>
        /// property of a <see cref="JsonReader"/> to true when <see cref="Toggler{T}.Toggle"/>
        /// is called.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> whose <see cref="JsonReader.DecryptReads"/>
        /// property is toggled.</param>
        public DecryptReadsToggler(JsonReader reader, string path)
            : base(x => reader.SetDecryptReads(x, path), reader.DecryptReads, true)
        {
        }
    }
}