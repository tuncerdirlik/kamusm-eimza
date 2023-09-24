using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.smartcard.pkcs11;
using tr.gov.tubitak.uekae.esya.asn.util;

/**
 * Creates content info structures that has several paralel signatures
 */

namespace tr.gov.tubitak.uekae.esya.api.cades.example.sign
{
    [TestFixture]
    public class ParallelSign : CadesSampleBase
    {
        public readonly int SIGNATURE_COUNT = 5;

        private readonly DirectoryInfo testDataDirectory = Directory.CreateDirectory(getTestDataFolder());

        /**
         * adds a parallel signature to a signature structure.
         * @param content
         * @param sign
         * @param signer
         * @param cert
         * @return
         * @throws Exception
         */

        private byte[] sign(byte[] content, byte[] sign, BaseSigner signer, ECertificate cert)
        {
            BaseSignedData bs;

            Dictionary<string, object> params_ = new Dictionary<string, object>();

            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();


            //If sign is null, there is no signature before.
            if (sign == null)
            {
                bs = new BaseSignedData();
                bs.addContent(new SignableByteArray(content));
            }
            else
            {
                bs = new BaseSignedData(sign);
            }

            //add signer
            bs.addSigner(ESignatureType.TYPE_BES, cert, signer, null, params_);

            //returns content info
            return bs.getEncoded();
        }

        /**
         * Creates a signature structure that has two different signers. Each signer has signatures as much as SIGNATURE_COUNT
         * variable.
         * @throws Exception
         */


        [Test]
        public void testParallelSign()
        {
            bool checkQCStatement = isQualified();

            SmartCardManager scm = SmartCardManager.getInstance();
            //Get Non-Qualified certificate.
            ECertificate cert = scm.getSignatureCertificate(checkQCStatement);
            BaseSigner signer = scm.getSigner(getPin(), cert);

            byte[] signature = null;
            byte[] content = Encoding.ASCII.GetBytes("test");
            for (int i = 0; i < SIGNATURE_COUNT; i++)
                signature = sign(content, signature, signer, cert);

            try
            {
                scm.logout();
            }
            catch (SmartCardException sce)
            {
                Console.WriteLine(sce);
            }

            AsnIO.dosyayaz(signature, testDataDirectory.FullName + @"\paralelSignatures.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signature, null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        [Test]
        public void testParallelSignWith2Signer()
        {
            bool checkQCStatement = isQualified();

            //Wants user to select two cards for parallel signatures. 
            Console.WriteLine("Select card - 1");
            //SmartCard - 1
            SmartCardManager scm1 = SmartCardManager.getInstance();
            //Get Non-Qualified certificate.
            ECertificate cert1 = scm1.getSignatureCertificate(checkQCStatement);
            BaseSigner signer1 = scm1.getSigner(getPin(), cert1);


            //reset to select second card.
            SmartCardManager.reset();


            Console.WriteLine("Select card - 2");
            //SmartCard - 2 
            SmartCardManager scm2 = SmartCardManager.getInstance();
            //Get Non-Qualified certificate.
            ECertificate cert2 = scm2.getSignatureCertificate(checkQCStatement);
            BaseSigner signer2 = scm2.getSigner(getPin(), cert2);

            SmartCardManager.reset();


            byte[] signature = null;
            byte[] content = Encoding.ASCII.GetBytes("test");
            for (int i = 0; i < SIGNATURE_COUNT; i++)
            {
                //adds a paralel signature of signer one
                signature = sign(content, signature, signer1, cert1);
                //adds a paralel signature of signer two
                signature = sign(content, signature, signer2, cert2);
            }

            try
            {
                scm1.logout();
                scm2.logout();
            }
            catch (SmartCardException sce)
            {
                Console.WriteLine(sce);
            }


            AsnIO.dosyayaz(signature, testDataDirectory.FullName + @"\paralelSignatures2.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signature, null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }
    }
}