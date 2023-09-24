using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.asn.util;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.sign
{
    [TestFixture]
    public class ExternalContentSign : CadesSampleBase
    {
        private readonly string docFile = "D:\\Docs\\MA3API.docx";
        private readonly string movieFile = "D:\\Movie\\DocumentaryMovie.mkv";

        private readonly DirectoryInfo testDataDirectory = Directory.CreateDirectory(getRootDir() + @"\testVerileri");

        /**
         * creates BES type signature with huge external content and validate it. Use external signature for huge files.
         * @throws Exception
         */

        [Test]
        public void testSignHugeFile()
        {
            BaseSignedData bs = new BaseSignedData();

            FileInfo file = new FileInfo(movieFile);
            ISignable externalContent = new SignableFile(file, 2048);

            //create parameters necessary for signature creation
            Dictionary<string, object> params_ = new Dictionary<string, object>();

            //necessary for certificate validation.By default,certificate validation is done.But if the user 
            //does not want certificate validation,he can add P_VALIDATE_CERTIFICATE_BEFORE_SIGNING parameter with its value set to false
            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            //By default, QC statement is checked,and signature wont be created if it is not a qualified certificate
            bool checkQCStatement = isQualified();
            params_[EParameters.P_VALIDATE_CERTIFICATE_BEFORE_SIGNING] = false;

            bs.addContent(externalContent, false);

            //Get qualified or non-qualified certificate.
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(checkQCStatement);

            BaseSigner signer = SmartCardManager.getInstance().getSigner(getPin(), cert);

            //add signer
            //Since the specified attributes are mandatory for bes,null is given as parameter for optional attributes
            bs.addSigner(ESignatureType.TYPE_BES, cert, signer, null, params_);

            SmartCardManager.getInstance().logout();

            byte[] signature = bs.getEncoded();

            //write the contentinfo to file
            AsnIO.dosyayaz(signature, testDataDirectory.FullName + @"\HugeExternalContent.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signature, externalContent);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        /**
         * creates BES type signature with normal size external content and validate it.
         * @throws Exception
         */

        [Test]
        public void testSignSmallFile()
        {
            BaseSignedData bs = new BaseSignedData();

            FileInfo file = new FileInfo(docFile);
            ISignable externalContent = new SignableFile(file, 2048);

            //create parameters necessary for signature creation
            Dictionary<string, object> params_ = new Dictionary<string, object>();


            //necessary for certificate validation.By default,certificate validation is done.But if the user 
            //does not want certificate validation,he can add P_VALIDATE_CERTIFICATE_BEFORE_SIGNING parameter with its value set to false
            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            //By default, QC statement is checked,and signature wont be created if it is not a qualified certificate
            bool checkQCStatement = isQualified();
            bs.addContent(externalContent, false);


            //Get qualified or non-qualified certificate.
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(checkQCStatement);
            BaseSigner signer = SmartCardManager.getInstance().getSigner(getPin(), cert);


            //add signer
            //Since the specified attributes are mandatory for bes,null is given as parameter for optional attributes
            bs.addSigner(ESignatureType.TYPE_BES, cert, signer, null, params_);

            SmartCardManager.getInstance().logout();

            byte[] signature = bs.getEncoded();

            //write the contentinfo to file
            AsnIO.dosyayaz(signature, testDataDirectory.FullName + @"\SmallExternalContent.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(signature, externalContent);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }
    }
}