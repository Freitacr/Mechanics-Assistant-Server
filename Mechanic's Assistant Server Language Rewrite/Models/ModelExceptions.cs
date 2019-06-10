using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicsAssistantServer.Models
{
    public class InvalidDataFormatException : Exception
    {
        public InvalidDataFormatException(string message) : base(message) { }
    }

    public class InvalidPredictionParameterException : Exception
    {
        public InvalidPredictionParameterException(string message) : base(message) { }
    }
}
