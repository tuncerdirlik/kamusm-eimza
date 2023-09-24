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
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;
using tr.gov.tubitak.uekae.esya.asn.util;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.sign
{
    [TestFixture]
    public class ESTSign : CadesSampleBase
    {
        private readonly DirectoryInfo testDataDirectory = Directory.CreateDirectory(getTestDataFolder());

        /**
	     * creates EST type signature and validate it.
	     * @throws Exception
	     */

        [Test]
        public void testEstSign()
        {
            BaseSignedData bs = new BaseSignedData();

            ISignable content = new SignableByteArray(Encoding.ASCII.GetBytes("test"));
            bs.addContent(content);

            Dictionary<string, object> params_ = new Dictionary<string, object>();

            //if the user does not want certificate validation,he can add 
            //P_VALIDATE_CERTIFICATE_BEFORE_SIGNING parameter with its value set to false
            params_[EParameters.P_VALIDATE_CERTIFICATE_BEFORE_SIGNING] = false;

            //if the user want to do timestamp validation while generating signature,he can add
            //P_VALIDATE_TIMESTAMP_WHILE_SIGNING parameter with its value set to true
            //params_[EParameters.P_VALIDATE_TIMESTAMP_WHILE_SIGNING]= true;

            bool checkQCStatement = isQualified();
            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            //necessary for getting signature time stamp.
            //for getting test TimeStamp or qualified TimeStamp account, mail to bilgi@kamusm.gov.tr
            TSSettings tsSettings = getTSSettings();
            params_[EParameters.P_TSS_INFO] = tsSettings;


            //Get qualified or non-qualified certificate.
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(checkQCStatement);
            BaseSigner signer = SmartCardManager.getInstance().getSigner(getPin(), cert);

            //add signer
            bs.addSigner(ESignatureType.TYPE_EST, cert, signer, null, params_);

            SmartCardManager.getInstance().logout();

            byte[] signedDocument = bs.getEncoded();

            AsnIO.dosyayaz(signedDocument, testDataDirectory.FullName + @"\EST-1.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signedDocument, null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }
    }
}