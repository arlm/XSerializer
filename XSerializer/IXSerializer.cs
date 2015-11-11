using System.IO;

namespace XSerializer
{
    /// <summary>
    /// An interface suitable for serializing a object of a specific type.
    /// </summary>
    public interface IXSerializer
    {
        /// <summary>
        /// Serialize the given object to a string.
        /// </summary>
        /// <param name="instance">The object to serialize.</param>
        /// <returns>A string representation of the object.</returns>
        string Serialize(object instance);
        
        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        void Serialize(Stream stream, object instance);

        /// <summary>
        /// Serialize the given object to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to serialize the object to.</param>
        /// <param name="instance">The object to serialize.</param>
        void Serialize(TextWriter textWriter, object instance);

        /// <summary>
        /// Deserialize an object from a string.
        /// </summary>
        /// <param name="data">A string representation of an object.</param>
        /// <returns>An object created from the string.</returns>
        object Deserialize(string data);

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="Stream"/>.</returns>
        object Deserialize(Stream stream);

        /// <summary>
        /// Deserialize an object from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="textReader">
        /// A <see cref="TextReader"/> that when read, contains a representation of an object.
        /// </param>
        /// <returns>An object created from the <see cref="TextReader"/>.</returns>
        object Deserialize(TextReader textReader);
    }
}