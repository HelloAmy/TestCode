using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace YZ.Utility
{
    public class BusinessException : Exception
    {
        public int Code { get; private set; }

        /// <summary>
        /// Initializes a new instance of <c>BusinessException</c> class.
        /// </summary>
        public BusinessException() : base() { }
        /// <summary>
        /// Initializes a new instance of <c>BusinessException</c> class.
        /// </summary>
        /// <param name="message">The error message to be provided to the exception.</param>
        public BusinessException(string message, int code = 0) : base(message)
        {
            this.Code = code;
        }
        ///// <summary>
        ///// Initializes a new instance of <c>BusinessException</c> class.
        ///// </summary>
        ///// <param name="message">The error message to be provided to the exception.</param>
        ///// <param name="innerException">The inner exception which causes this exception to occur.</param>
        //public BusinessException(string message, int code = 0, Exception innerException = null) : base(message, innerException)
        //{
        //    this.Code = code;
        //}

        ///// <summary>
        ///// Initializes a new instance of <c>BusinessException</c> class.
        ///// </summary>
        ///// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        ///// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        //protected BusinessException(int code, SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //    this.Code = code;
        //}
    }

}
