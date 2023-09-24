using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.asn.util;

/**
 * Conversion to ESA. ESA signature can not be created directly. They must be converted from other signature types.
 * Firstly run sign operations in order to create signatures to be converted. 
 *
 */

namespace tr.gov.tubitak.uekae.esya.api.cades.example.convert
{
    [TestFixture]
    public class Converts : CadesSampleBase
    {
        private readonly string docFile = "D:\\Docs\\MA3API.docx";
        private readonly string movieFile = "D:\\Movie\\DocumentaryMovie.mkv";

        private readonly DirectoryInfo testDataDirectory = Directory.CreateDirectory(getTestDataFolder());

        private readonly string signatureofHugeFile = getTestDataFolder() + "HugeExternalContent.p7s";
        private readonly string signatureofSmallFile = getTestDataFolder() + "SmallExternalContent.p7s";

        /**
         * Converting BES signature to ESA
         * @throws Exception
         */

        [Test]
        public void testConvertBES_1()
        {
            byte[] content = AsnIO.dosyadanOKU(getTestDataFolder() + "BES-1.p7s");

            BaseSignedData bs = new BaseSignedData(content);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Several time stamps are needed while converting to ESA; so time stamps settings must be given.
            parameters[EParameters.P_TSS_INFO] = getTSSettings();

            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            bs.getSignerList()[0].convert(ESignatureType.TYPE_ESA, parameters);

            AsnIO.dosyayaz(bs.getEncoded(), testDataDirectory.FullName + @"\ESA-1.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(bs.getEncoded(), null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        /**
         * Converting external signature to ESA.
         * @throws Exception
         */

        [Test]
        public void testConvertExternalContentSignature_3()
        {
            FileInfo file = new FileInfo(docFile);
            ISignable signable = new SignableFile(file, 2048);

            byte[] content = AsnIO.dosyadanOKU(signatureofSmallFile);
            BaseSignedData bs = new BaseSignedData(content);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Archive time stamp is added to signature, so time stamp settings are needed.
            parameters[EParameters.P_TSS_INFO] = getTSSettings();

            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();
            parameters[EParameters.P_EXTERNAL_CONTENT] = signable;

            bs.getSignerList()[0].convert(ESignatureType.TYPE_ESA, parameters);

            AsnIO.dosyayaz(bs.getEncoded(), testDataDirectory.FullName + @"\ESA-3.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(bs.getEncoded(), signable);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        /**
         * Converting external signature of a huge file to ESA.
         * @throws Exception
         */

        [Test]
        public void testConvertHugeExternalContentSignature_4()
        {
            FileInfo file = new FileInfo(movieFile);
            ISignable signable = new SignableFile(file, 2048);

            byte[] content = AsnIO.dosyadanOKU(signatureofHugeFile);
            BaseSignedData bs = new BaseSignedData(content);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Archive time stamp is added to signature, so time stamp settings are needed.
            parameters[EParameters.P_TSS_INFO] = getTSSettings();

            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();
            parameters[EParameters.P_EXTERNAL_CONTENT] = signable;

            bs.getSignerList()[0].convert(ESignatureType.TYPE_ESA, parameters);

            AsnIO.dosyayaz(bs.getEncoded(), testDataDirectory.FullName + @"\ESA-4.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();
            SignedDataValidationResult sdvr = validationUtil.validate(bs.getEncoded(), signable);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }

        /**
         * Converting XLong signature to ESA.
         * @throws Exception
         */

        [Test]
        public void testConvertXLong_2()
        {
            byte[] content = AsnIO.dosyadanOKU(getTestDataFolder() + "ESXLong-1.p7s");
            BaseSignedData bs = new BaseSignedData(content);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Archive time stamp is added to signature, so time stamp settings are needed.
            parameters[EParameters.P_TSS_INFO] = getTSSettings();

            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            bs.getSignerList()[0].convert(ESignatureType.TYPE_ESA, parameters);

            AsnIO.dosyayaz(bs.getEncoded(), testDataDirectory.FullName + @"\ESA-2.p7s");

            CadesSignatureValidation validationUtil = new CadesSignatureValidation();

            SignedDataValidationResult sdvr = validationUtil.validate(bs.getEncoded(), null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());
        }     
    }
}