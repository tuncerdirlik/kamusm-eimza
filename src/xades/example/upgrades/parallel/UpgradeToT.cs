using System.Xml;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.upgrades.parallel
{
    /**
     * Provides functions for upgrade of parallel BES to type T
     */

    [TestFixture]
    public class UpgradeToT : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "t_from_bes_parallel.xml";

        /**
         * Upgrade parallel BES to type T
         */

        [Test]
        public void upgradeBESToT()
        {
            // create context with working dir
            Context context = createContext();

            // parse the previously created enveloped signature
            XmlDocument document = parseDoc("/testVerileri/parallel_detached.xml");

            // get and upgrade the signature 1 in DOM document
            XMLSignature signature1 = readSignature(document, context, 0);
            signature1.upgrade(signature.SignatureType.ES_T);

            // get and upgrade the signature 2 in DOM document
            XMLSignature signature2 = readSignature(document, context, 1);
            signature2.upgrade(signature.SignatureType.ES_T);

            // writing enveloped XML to the file
            document.Save(getTestDataFolder() + SIGNATURE_FILENAME);

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validateParallel(SIGNATURE_FILENAME);
        }
    }
}