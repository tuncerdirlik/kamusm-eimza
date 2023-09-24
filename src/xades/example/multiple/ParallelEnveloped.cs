using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.resolver;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.multiple
{
    /**
     * Parallel signature enveloped sample
     */

    [TestFixture]
    public class ParallelEnveloped : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "parallel_enveloped.xml";

        /**
         * Creates two signatures in a document, that signs same inner data
         */

        [Test]
        public void createParallelEnveloped()
        {
            Context context = createContext();

            SignedDocument signatures = new SignedDocument(context);

            Document doc = Resolver.resolve("./sample.xml", context);
            string fragment = signatures.addDocument(doc);

            XMLSignature signature1 = signatures.createSignature();

            // add document as inner reference
            signature1.addDocument("#" + fragment, "text/xml", false);

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            signature1.addKeyInfo(cert);

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature1.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            XMLSignature signature2 = signatures.createSignature();

            // add document as inner reference
            signature2.addDocument("#" + fragment, "text/plain", false);

            // add certificate to show who signed the document
            signature2.addKeyInfo(cert);

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature2.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            // write combined document
            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signatures.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validateParallel(SIGNATURE_FILENAME);
        }
    }
}