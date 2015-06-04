using System;

namespace XSerializer
{
    /// <summary>
    /// An object that holds an arbitrary value, to be used by one or more encrypt/decrypt
    /// operations within a single serialization operation.
    /// </summary>
    public sealed class SerializationState
    {
        private readonly object _locker = new object();

        private volatile object _value;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="getValue">A function that produces the value. This function will only be executed if this method has never been called before.</param>
        /// <returns>The value</returns>
        public T Get<T>(Func<T> getValue)
        {
            if (getValue == null) throw new ArgumentNullException("getValue");

            if (_value == null)
            {
                lock (_locker)
                {
                    if (_value == null)
                    {
                        var instance = getValue();

                        if (instance == null)
                        {
                            throw new ArgumentException("The 'getValue' function must return a non-null value.", "getValue");
                        }

                        _value = instance;
                    }
                }
            }

            if (!(_value is T))
            {
                throw new InvalidOperationException("The previously created value is not assignable to type " + typeof(T));
            }

            return (T)_value;
        }
    }
}