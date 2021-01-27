using System;
using System.Collections.Generic;
using System.Text;

namespace WebRPC
{
    public class RPCException : Exception
    {
        public RPCException()
        {
        }
        public RPCException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ValidationException : Exception
    {
    }
}
