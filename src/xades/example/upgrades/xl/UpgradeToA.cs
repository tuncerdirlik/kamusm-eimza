using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using SignatureType = tr.gov.tubitak.uekae.esya.api.signature.SignatureType;

namespace xmlsig.samples.upgrades.xl
{
    /**
     * Provides upgrade function from XL to A
     */

    [TestFixture]
    public class UpgradeToA : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "a_from_xl.xml";

        /**
         * Upgrades XL to A. XL needs to be provided before upgrade process.
         * It can be created in formats.XL.
         */

        [Test]
        public void upgradeXLToA()
        {
            // create context with working dir
            Context context = createContext();

            // read signature to be upgraded
            XMLSignature signature = XMLSignature.parse(new FileDocument(new FileInfo(getTestDataFolder() + "xl.xml")),
                context);

            // upgrade to A
            signature.upgrade(SignatureType.ES_A);

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}