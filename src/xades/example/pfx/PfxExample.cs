using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.pfx
{
    /**
     * Creating electronic signature using pfx
     */

    [TestFixture]
    public class PfxExample : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "pfxexample.xml";

        /**
         * Creates detached BES using pfx
         */

        [Test]
        public void createDetachedBesWithPfx()
        {
            // create context with working dir
            Context context = createContext();

            // create signature according to context,
            // with default type (XADES_BES)
            XMLSignature signature = new XMLSignature(context);

            // add document as reference, but do not embed it
            // into the signature (embed=false)
            signature.addDocument("./sample.txt", "text/plain", false);

            // add signer's certificate
            ECertificate pfxSignersCertificate = getPfxSigner().getSignersCertificate();
            signature.addKeyInfo(pfxSignersCertificate);

            // sign the document
            signature.sign(getPfxSigner());

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}