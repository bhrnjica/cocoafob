using System;
using System.Text;

namespace com.xk72.cocoafob
{


	using Base32 = org.apache.commons.codec.binary.Base32;
	using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
	using PEMReader = org.bouncycastle.openssl.PEMReader;

	/// <summary>
	/// Generate and verify CocoaFob licenses. Based on the PHP implementation by Sandro Noel.
	/// @author karlvr
	/// 
	/// </summary>
	public class LicenseGenerator
	{

		private DSAPrivateKey privateKey;
		private DSAPublicKey publicKey;
		private SecureRandom random;

		static LicenseGenerator()
		{
			Security.addProvider(new BouncyCastleProvider());
		}

		protected internal LicenseGenerator()
		{
			random = new SecureRandom();
		}

		/// <summary>
		/// Construct the LicenseGenerator with a URL that points to either the private key or public key.
		/// Pass the private key for making and verifying licenses. Pass the public key for verifying only.
		/// If you this code will go onto a user's machine you MUST NOT include the private key, only include
		/// the public key in this case. </summary>
		/// <param name="keyURL"> </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public LicenseGenerator(java.net.URL keyURL) throws java.io.IOException
		public LicenseGenerator(URL keyURL) : this()
		{
			initKeys(keyURL.openStream());
		}

		/// <summary>
		/// Construct the LicenseGenerator with an InputStream of either the private key or public key.
		/// Pass the private key for making and verifying licenses. Pass the public key for verifying only.
		/// If you this code will go onto a user's machine you MUST NOT include the private key, only include
		/// the public key in this case. </summary>
		/// <param name="keyURL"> </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public LicenseGenerator(java.io.InputStream keyInputStream) throws java.io.IOException
		public LicenseGenerator(System.IO.Stream keyInputStream) : this()
		{
			initKeys(keyInputStream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initKeys(java.io.InputStream keyInputStream) throws java.io.IOException
		private void initKeys(System.IO.Stream keyInputStream)
		{
			object readKey = readKey(keyInputStream);
			if (readKey is KeyPair)
			{
				KeyPair keyPair = (KeyPair) readKey;
				privateKey = (DSAPrivateKey) keyPair.Private;
				publicKey = (DSAPublicKey) keyPair.Public;
			}
			else if (readKey is DSAPublicKey)
			{
				publicKey = (DSAPublicKey) readKey;
			}
			else
			{
				throw new System.ArgumentException("The supplied key stream didn't contain a public or private key: " + readKey.GetType());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Object readKey(java.io.InputStream privateKeyInputSteam) throws java.io.IOException
		private object readKey(System.IO.Stream privateKeyInputSteam)
		{
			PEMReader pemReader = new PEMReader(new System.IO.StreamReader(new BufferedInputStream(privateKeyInputSteam)));
			try
			{
				return pemReader.readObject();
			}
			finally
			{
				pemReader.close();
			}
		}

		/// <summary>
		/// Make and return a license for the given <seealso cref="LicenseData"/>. </summary>
		/// <param name="licenseData">
		/// @return </param>
		/// <exception cref="LicenseGeneratorException"> If the generation encounters an error, usually due to invalid input. </exception>
		/// <exception cref="IllegalStateException"> If the generator is not setup correctly to make licenses. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String makeLicense(LicenseData licenseData) throws LicenseGeneratorException, IllegalStateException
		public virtual string makeLicense(LicenseData licenseData)
		{
			if (!CanMakeLicenses)
			{
				throw new System.InvalidOperationException("The LicenseGenerator cannot make licenses as it was not configured with a private key");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String stringData = licenseData.toLicenseStringData();
			string stringData = licenseData.toLicenseStringData();

			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.security.Signature dsa = java.security.Signature.getInstance("SHA1withDSA", "SUN");
				Signature dsa = Signature.getInstance("SHA1withDSA", "SUN");
				dsa.initSign(privateKey, random);
				dsa.update(stringData.GetBytes(Encoding.UTF8));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] signed = dsa.sign();
				sbyte[] signed = dsa.sign();

				/* base 32 encode the signature */
				string result = (new Base32()).encodeAsString(signed);

				/* replace O with 8 and I with 9 */
				result = result.Replace("O", "8").Replace("I", "9");

				/* remove padding if any. */
				result = result.Replace("=", "");

				/* chunk with dashes */
				result = split(result, 5);
				return result;
			}
			catch (NoSuchAlgorithmException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (NoSuchProviderException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (InvalidKeyException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (SignatureException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (UnsupportedEncodingException e)
			{
				throw new LicenseGeneratorException(e);
			}
		}

		/// <summary>
		/// Verify the given license for the given <seealso cref="LicenseData"/>. </summary>
		/// <param name="licenseData"> </param>
		/// <param name="license"> </param>
		/// <returns> Whether the license verified successfully. </returns>
		/// <exception cref="LicenseGeneratorException"> If the verification encounters an error, usually due to invalid input. You MUST check the return value of this method if no exception is thrown. </exception>
		/// <exception cref="IllegalStateException"> If the generator is not setup correctly to verify licenses. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean verifyLicense(LicenseData licenseData, String license) throws LicenseGeneratorException, IllegalStateException
		public virtual bool verifyLicense(LicenseData licenseData, string license)
		{
			if (!CanVerifyLicenses)
			{
				throw new System.InvalidOperationException("The LicenseGenerator cannot verify licenses as it was not configured with a public key");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String stringData = licenseData.toLicenseStringData();
			string stringData = licenseData.toLicenseStringData();

			/* replace O with 8 and I with 9 */
			string licenseSignature = license.Replace("8", "O").Replace("9", "I");

			/* remove dashes */
			licenseSignature = licenseSignature.Replace("-", "");

			/* Pad the output length to a multiple of 8 with '=' characters */
			while (licenseSignature.Length % 8 != 0)
			{
				licenseSignature += "=";
			}

			sbyte[] decoded = (new Base32()).decode(licenseSignature);
			try
			{
				Signature dsa = Signature.getInstance("SHA1withDSA", "SUN");
				dsa.initVerify(publicKey);
				dsa.update(stringData.GetBytes(Encoding.UTF8));
				return dsa.verify(decoded);
			}
			catch (NoSuchAlgorithmException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (NoSuchProviderException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (InvalidKeyException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (SignatureException e)
			{
				throw new LicenseGeneratorException(e);
			}
			catch (UnsupportedEncodingException e)
			{
				throw new LicenseGeneratorException(e);
			}
		}

		private string split(string str, int chunkSize)
		{
			StringBuilder result = new StringBuilder();
			int i = 0;
			while (i < str.Length)
			{
				if (i > 0)
				{
					result.Append('-');
				}
				int next = Math.Min(i + chunkSize, str.Length);
				result.Append(str.Substring(i, next - i));
				i = next;
			}
			return result.ToString();
		}

		public virtual bool CanMakeLicenses
		{
			get
			{
				return privateKey != null;
			}
		}

		public virtual bool CanVerifyLicenses
		{
			get
			{
				return publicKey != null;
			}
		}

	}

}