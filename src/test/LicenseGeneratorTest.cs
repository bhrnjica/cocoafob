using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System;
using System.IO;
using cocoafob;

namespace com.xk72.cocoafob
{
    [TestClass]
	public class LicenseGeneratorTest
	{

        [TestMethod]
		public virtual void testPrivateKey()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
			Assert.IsTrue(lg.CanMakeLicenses);
			Assert.IsTrue(lg.CanVerifyLicenses);
		}
        [TestMethod]
        public virtual void testPublicKey()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("pubkey.pem"));
			Assert.IsFalse(lg.CanMakeLicenses);
			Assert.IsTrue(lg.CanVerifyLicenses);
		}

        [TestMethod]
        public virtual void testMakeLicense1()
        {
            LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
            string license = lg.makeLicense(new LicenseData(null, null, null));
            Assert.IsTrue(license.Length > 0);
        }


        [TestMethod]
        public virtual void testMakeLicense()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
			string license = lg.makeLicense(new LicenseData("Test", "Karl", "karl@example.com"));
			Assert.IsTrue(license.Length > 0);
		}
        [TestMethod]
        public virtual void testVerifyLicense()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
			LicenseData licenseData = new LicenseData("Test", "Karl", "karl@example.com");
            //
			string license = lg.makeLicense(licenseData);
			bool verified = lg.verifyLicense(licenseData, license);
			Assert.IsTrue(verified);
		}


       

        [TestMethod]
        public virtual void testVerifyLicense2()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
			LicenseData licenseData = new LicenseData("Test", "Karl");
			string license = lg.makeLicense(licenseData);
			bool verified = lg.verifyLicense(licenseData, license);
			Assert.IsTrue(verified);
		}

        [TestMethod]
        public virtual void testFailedVerifyLicense()
		{
			LicenseGenerator lg = new LicenseGenerator(getResource("privkey.pem"));
			LicenseData licenseData = new LicenseData("Test", "Karl");
			Assert.IsTrue(lg.verifyLicense(licenseData, "GAWQE-F9AVF-8YSF3-NBDUH-C6M2J-JYAYC-X692H-H65KR-A9KAQ-R9SB7-A374H-T6AH3-87TAB-CVV6K-SKUGG-A"));
			Assert.IsTrue(lg.verifyLicense(licenseData, "GAWQE-F9AVF-8YSF3-NBDUH-C6M2J-JYAYC-X692H-H65KR-A9KAQ-R9SB7-A374H-T6AH3-87TAB-CVV6K-SKAGG-A"));
			Assert.IsTrue(lg.verifyLicense(licenseData, "GAWQE-F9AVF-8YSF3-NBDUH-C6M2J-JYAYC-X692H-H65KR-A9KAQ-R9SB7-A374H-T6AH3-87TAB-DVV6K-SKUGG-A"));
		}

      
        private string getResource(string fileName)
        {
            var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"resources", fileName);
            return filePath;
        }
    }

}