using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.multiple
{
    /**
     * Counter signature to a parallel signature sample
     */

    [TestFixture]
    public class CounterParallel : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "counter_parallel.xml";

        /**
         * Adds counter signature to a parallel detached one
         * @throws Exception
         */

        [Test]
        public void signCounterParallel()
        {
            Context context = createContext();

            // read previously created signature
            SignedDocument signedDocument =
                new SignedDocument(
                    new FileDocument(new FileInfo(getTestDataFolder() + ParallelDetached.SIGNATURE_FILENAME)),
                    context);

            // get First signature
            XMLSignature signature = signedDocument.getSignature(0);

            // create counter signature
            XMLSignature counterSignature = signature.createCounterSignature();

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            counterSignature.addKeyInfo(cert);

            // now sign it by using smart card
            // specifiy the PIN before sign
            counterSignature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            // signed doc contains both previous signature and now a counter signature
            // in first signature
            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signedDocument.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validateParallel(SIGNATURE_FILENAME);
        }
    }
}