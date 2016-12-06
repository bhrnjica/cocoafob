using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;
using Org.BouncyCastle.OpenSsl;

namespace cocoafob
{
    /// <summary>
    /// Generate and verify CocoaFob licenses. Based on the PHP implementation by Sandro Noel.
    /// @author karlvr
    /// 
    /// </summary>
    public class LicenseGenerator
	{

		private AsymmetricKeyParameter privateKey;
		private AsymmetricKeyParameter publicKey;
		private SecureRandom random;

		static LicenseGenerator()
		{
			//Security.addProvider(new BouncyCastleProvider());
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
		/// <param name="keyPath"> </param>
		/// <exception cref="IOException"> </exception>
		public LicenseGenerator(string keyPath) : this()
		{
            var stream = new StreamReader(keyPath);
			initKeys(stream);
		}

		/// <summary>
		/// Construct the LicenseGenerator with an InputStream of either the private key or public key.
		/// Pass the private key for making and verifying licenses. Pass the public key for verifying only.
		/// If you this code will go onto a user's machine you MUST NOT include the private key, only include
		/// the public key in this case. </summary>
		/// <param name="keyURL"> </param>
		/// <exception cref="IOException"> </exception>
		public LicenseGenerator(System.IO.StreamReader keyInputStream) : this()
		{
			initKeys(keyInputStream);
		}

		private void initKeys(System.IO.StreamReader keyInputStream)
		{
			object readKey1 = readKey(keyInputStream);
			if (readKey1 is AsymmetricCipherKeyPair)
			{
                var keyPair = (AsymmetricCipherKeyPair) readKey1;
				privateKey = keyPair.Private;
				publicKey = keyPair.Public;
			}
			else if (readKey1 is DsaPublicKeyParameters)
			{
				publicKey = (DsaPublicKeyParameters) readKey1;
			}
			else
			{
				throw new System.ArgumentException("The supplied key stream didn't contain a public or private key: " + readKey1.GetType());
			}
		}

      	private object readKey(System.IO.StreamReader privateKeyInputStream)
		{
            //PEMReader pemReader = new PEMReader(new System.IO.StreamReader(new BufferedInputStream(privateKeyInputSteam)));
            PemReader pemReader = new PemReader(privateKeyInputStream);
            try
			{
				return pemReader.ReadObject();
			}
			finally
			{
                privateKeyInputStream.Close();
                //pemReader.Close();
            }
		}

		/// <summary>
		/// Make and return a license for the given <seealso cref="LicenseData"/>. </summary>
		/// <param name="licenseData">
		/// @return </param>
		/// <exception cref="LicenseGeneratorException"> If the generation encounters an error, usually due to invalid input. </exception>
		/// <exception cref="IllegalStateException"> If the generator is not setup correctly to make licenses. </exception>
public string makeLicense(LicenseData licenseData)
{
	if (!CanMakeLicenses)
	{
		throw new System.InvalidOperationException("The LicenseGenerator cannot make licenses as it was not configured with a private key");
	}
	try
	{
        //
        var dsa = SignerUtilities.GetSigner("SHA1withDSA");
        dsa.Init(true, privateKey);

        //
        string stringData = licenseData.toLicenseStringData();
        byte[] licBytes = Encoding.UTF8.GetBytes(stringData);
        dsa.BlockUpdate(licBytes, 0, licBytes.Length);

        //
        byte[] signed = dsa.GenerateSignature();


        string license = ToLicenseKey(signed);


        return license;
	}
    catch (Exception e)
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
        public virtual bool verifyLicense(LicenseData licenseData, string license)
        {
	        if (!CanVerifyLicenses)
	        {
		        throw new System.InvalidOperationException("The LicenseGenerator cannot verify licenses as it was not configured with a public key");
	        }
            try
	        {
                //Signature dsa = Signature.getInstance("SHA1withDSA", "SUN");
                var dsa = SignerUtilities.GetSigner("SHA1withDSA");
                dsa.Init(false, publicKey);

                //
                string stringData = licenseData.toLicenseStringData();
                byte[] msgBytes = Encoding.UTF8.GetBytes(stringData);
                dsa.BlockUpdate(msgBytes, 0, msgBytes.Length);


                var dec = FromLicenseKey(license);
                var retVal = dsa.VerifySignature(dec);
                //
                return retVal; 
	        }
	        catch (InvalidKeyException e)
	        {
		        throw new LicenseGeneratorException(e);
	        }
	        catch (SignatureException e)
	        {
		        throw new LicenseGeneratorException(e);
	        }
            catch (Exception e)
            {
                throw new LicenseGeneratorException(e);
            }
        }




        private string ToLicenseKey(byte[] signature)
        {
            /* base 32 encode the signature */
            var result = Base32.ToString(signature);

            /* replace O with 8 and I with 9 */
            result = result.Replace("O", "8").Replace("I", "9");

            /* remove padding if any. */
            result = result.Replace("=", "");
           

            /* chunk with dashes */
            result = split(result, 5);
            return result;
        }

        private byte[] FromLicenseKey(string license)
        {

            /* replace O with 8 and I with 9 */
            var result = license.Replace("8", "O").Replace("9", "I").Replace("-","");

            /* Pad the output length to a multiple of 8 with '=' characters */
            while (result.Length % 8 != 0)
            {
                result += "=";
            }

            var retVal = Base32.ToBytes(result);

            return retVal;
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