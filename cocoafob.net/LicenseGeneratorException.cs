using System;

namespace cocoafob
{

	/// <summary>
	/// An error occurred in the license generation or verification. This generally means that the input
	/// was malformed somehow and should be rejected.
	/// @author karlvr
	/// 
	/// </summary>
	public class LicenseGeneratorException : Exception
	{

		public LicenseGeneratorException() : base()
		{
		}

		public LicenseGeneratorException(string message, Exception cause) : base(message, cause)
		{
		}

		public LicenseGeneratorException(string message) : base(message)
		{
		}

		public LicenseGeneratorException(Exception cause) : base(cause.Message)
		{
		}

	}

}