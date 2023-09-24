using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.profile;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;
using tr.gov.tubitak.uekae.esya.asn.util;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.sign.profil
{
    [TestFixture]
    public class P3 : CadesSampleBase
    {
        private readonly DirectoryInfo testDataDirectory = Directory.CreateDirectory(getTestDataFolder());

        [Test]
        public void test()
        {
            testCreatePreP3();

            Console.Out.WriteLine("Yeni sil yayınlandıktan sonra 'upgrade' işlemi yapılmalıdır.");

            testUpgradeP3();
        }


        public void testCreatePreP3()
        {
            BaseSignedData bs = new BaseSignedData();

            ISignable content = new SignableByteArray(Encoding.ASCII.GetBytes("test"));
            bs.addContent(content);

            List<IAttribute> optionalAttributes = new List<IAttribute>();
            optionalAttributes.Add(new SigningTimeAttr(DateTime.UtcNow));
            optionalAttributes.Add(new SignaturePolicyIdentifierAttr(TurkishESigProfile.P3_1));

            Dictionary<string, object> params_ = new Dictionary<string, object>();

            //if the user does not want certificate validation,he can add 
            //P_VALIDATE_CERTIFICATE_BEFORE_SIGNING parameter with its value set to false
            params_[EParameters.P_VALIDATE_CERTIFICATE_BEFORE_SIGNING] = false;

            bool checkQCStatement = isQualified();
            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            //necessary for getting signature time stamp.
            TSSettings tsSettings = getTSSettings();
            params_[EParameters.P_TSS_INFO] = tsSettings;


            //Get qualified or non-qualified certificate.
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(checkQCStatement);
            BaseSigner signer = SmartCardManager.getInstance().getSigner(getPin(), cert);

            //add signer
            bs.addSigner(ESignatureType.TYPE_EST, cert, signer, optionalAttributes, params_);

            SmartCardManager.getInstance().logout();

            byte[] signedDocument = bs.getEncoded();

            AsnIO.dosyayaz(signedDocument, testDataDirectory.FullName + @"\preP3.p7s");
        }

        public void testUpgradeP3()
        {
            byte[] signature = AsnIO.dosyadanOKU(getTestDataFolder() + "preP3.p7s");

            BaseSignedData bs = new BaseSignedData(signature);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            bs.getSignerList()[0].convert(ESignatureType.TYPE_ESXLong, parameters);

            byte[] signedDocument = bs.getEncoded();

            //write the contentinfo to file
            AsnIO.dosyayaz(signedDocument, getTestDataFolder() + "P3.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signedDocument, null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }
    }
}