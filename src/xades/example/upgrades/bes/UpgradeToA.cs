using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.upgrades.bes
{
    /**
     * Provides upgrade function from BES to A
     */

    [TestFixture]
    public class UpgradeToA : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "a_from_bes.xml";

        /**
         * Upgrades BES to A. BES needs to be provided before upgrade process.
         * It can be created in formats.BES.
         */

        [Test]
        public void upgradeBESToA()
        {
            // create context with working dir
            Context context = createContext();

            // read signature to be upgraded
            XMLSignature signature = XMLSignature.parse(
                new FileDocument(new FileInfo(getTestDataFolder() + "bes.xml")), context);

            // upgrade to A
            signature.upgrade(api.signature.SignatureType.ES_A);

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}