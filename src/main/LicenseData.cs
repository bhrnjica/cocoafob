﻿using System.Text;

namespace com.xk72.cocoafob
{

	/// <summary>
	/// Represents the data used as the string data input to the CocoaFob algorithm. Extend this class
	/// and override <seealso cref="LicenseData#toLicenseStringData()"/> to customise the string data to match your application.
	/// @author karlvr
	/// 
	/// </summary>
	public class LicenseData
	{

		protected internal string productCode;
		protected internal string name;
		protected internal string email;

		protected internal LicenseData() : base()
		{
		}

		public LicenseData(string productCode, string name) : base()
		{
			this.productCode = productCode;
			this.name = name;
		}

		public LicenseData(string productCode, string name, string email) : base()
		{
			this.productCode = productCode;
			this.name = name;
			this.email = email;
		}

		public virtual string ProductCode
		{
			get
			{
				return productCode;
			}
			set
			{
				this.productCode = value;
			}
		}


		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}


		public virtual string Email
		{
			get
			{
				return email;
			}
			set
			{
				this.email = value;
			}
		}


		/// <summary>
		/// Returns the string data input for the CocoaFob algorithm. This implementation returns a comma separated string
		/// including the <seealso cref="#productCode"/>, <seealso cref="#name"/> and <seealso cref="#email"/> if set.
		/// @return
		/// </summary>
		public virtual string toLicenseStringData()
		{
			StringBuilder result = new StringBuilder();
			result.Append(productCode);
			result.Append(',');
			result.Append(name);
			if (!string.ReferenceEquals(email, null))
			{
				result.Append(',');
				result.Append(email);
			}
			return result.ToString();
		}

	}

}