using System.IO;
using NUnit.Framework;
using UpgradeSignatureType = tr.gov.tubitak.uekae.esya.api.signature.SignatureType;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;


namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.upgrades.bes
{
    /**
     * Provides upgrade function from BES to XL
     */

    [TestFixture]
    public class UpgradeToXL : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "xl_from_bes.xml";

        /**
         * Upgrades BES to XL. BES needs to be provided before upgrade process.
         * It can be created in formats.BES.
         */

        [Test]
        public void upgradeBESToXL()
        {
            // create context with working dir
            Context context = createContext();

            // read signature to be upgraded
            XMLSignature signature = XMLSignature.parse(
                new FileDocument(new FileInfo(getTestDataFolder() + "bes.xml")), context);

            // upgrade to XL
            signature.upgrade(UpgradeSignatureType.ES_XL);

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}