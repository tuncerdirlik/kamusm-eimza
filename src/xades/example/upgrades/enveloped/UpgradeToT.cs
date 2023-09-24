using System.Xml;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.upgrades.enveloped
{
    /**
     * Provides functions for upgrade of enveloped BES to type T
     */

    [TestFixture]
    public class UpgradeToT : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "t_from_bes_enveloped.xml";

        /**
         * Upgrade enveloped BES to type T
         */

        [Test]
        public void upgradeBESToT()
        {
            // create context with working dir
            Context context = createContext();

            // parse the previously created enveloped signature
            XmlDocument document = parseDoc("/testVerileri/enveloped.xml");

            // get the signature in DOM document
            XMLSignature signature = readSignature(document, context);

            ValidationResult vr = signature.verify();

            // upgrade the signature to type T
            signature.upgrade(api.signature.SignatureType.ES_T);

            // writing enveloped XML to the file
            document.Save(getTestDataFolder() + SIGNATURE_FILENAME);

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}