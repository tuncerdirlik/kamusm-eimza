using System;
using System.Text;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;

/**
 * Test SignerManager class runs correctly.
 */

namespace tr.gov.tubitak.uekae.esya.api.cades.example.util
{
    [TestFixture]
    public class TestSignerManager : CadesSampleBase
    {
        /**
         * Tests BES sign
         * @throws Exception
         */

        [Test]
        public void testBESSign()
        {
            byte[] sign = null;
            SignableByteArray sba = new SignableByteArray(Encoding.ASCII.GetBytes("test"));

            DateTime? signingTime = DateTime.UtcNow;

            SignerManager signerManager = new SignerManager();
            for (int i = 0; i < 5; i++)
            {
                sign = signerManager.addSignerWithSigningTimeAttr(sba, sign, signingTime);
            }

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(sign, sba);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        /**
	 * Tests EST sign
	 * @throws Exception
	 */

        [Test]
        public void testEsxLong()
        {
            byte[] sign = null;
            SignableByteArray sba = new SignableByteArray(Encoding.ASCII.GetBytes("test"));
            ValidationPolicy policy = getPolicy();
            TSSettings tsSettings = getTSSettings();
            DigestAlg digestAlg = DigestAlg.SHA1;

            SignerManager signerManager = new SignerManager();
            for (int i = 0; i < 5; i++)
                sign = signerManager.addSigner(sba, sign);

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(sign, sba);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }
    }
}