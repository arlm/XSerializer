using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace XSerializer.Tests
{
    public static class ObjectManagerFixups
    {
        private static bool _canDoInvalidOperationFixup;

        static ObjectManagerFixups()
        {
            try
            {
                try
                {
                    Activator.CreateInstance(typeof(TargetInvocationInvalidOperationError));
                }
                catch (Exception e)
                {
                    var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
                    var mgr = new ObjectManager(null, ctx);
                    var si = new SerializationInfo(e.InnerException.GetType(), new FormatterConverter());

                    e.InnerException.GetObjectData(si, ctx);
                    mgr.RegisterObject(e.InnerException, 1, si); // prepare for SetObjectData
                    mgr.DoFixups(); // ObjectManager calls SetObjectData
                    _canDoInvalidOperationFixup = true;
                }
            }
            catch (TargetInvocationException ex)
                when (ex.InnerException is PlatformNotSupportedException)
            {
                _canDoInvalidOperationFixup = false;
            }
        }

        public static bool CanDoInvalidOperationFixup => _canDoInvalidOperationFixup;

        private class TargetInvocationInvalidOperationError
        {
            public TargetInvocationInvalidOperationError()
            {
                throw new InvalidOperationException("Virtual property must have non-empty XmlAttribute.");
            }
        }
    }
}
