using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.structures;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.resolver;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.multiple
{
    /**
     * Counter signature sample
     */

    [TestFixture]
    public class CounterDetached : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "counter_detached.xml";

        /**
         * Adds counter signature to a detached one
         * @throws Exception
         */

        [Test]
        public void signCounterDetached()
        {
            Context context = createContext();

            // read previously created signature, you need to run Detached first
            Document doc = Resolver.resolve(Detached.SIGNATURE_FILENAME, context);
            XMLSignature signature = XMLSignature.parse(doc, context);

            // create counter signature
            XMLSignature counterSignature = signature.createCounterSignature();

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            counterSignature.addKeyInfo(cert);

            // now sign it by using smart card
            // specifiy the PIN before sign
            counterSignature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            // signature contains itself and counter signature
            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}