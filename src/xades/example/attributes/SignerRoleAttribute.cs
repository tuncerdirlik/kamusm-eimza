using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.model.xades;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.attributes
{
    /**
     * BES with SignerRole attribute sample
     */

    [TestFixture]
    public class SignerRoleAttribute : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "signer_role.xml";

        /**
         * Creates detached BES with SignerRole attribute
         */

        [Test]
        public void createBESWithSignerRole()
        {
            // create context with working directory
            Context context = createContext();

            // create signature according to context,
            // with default type (XADES_BES)
            XMLSignature signature = new XMLSignature(context);

            // add document as reference, but do not embed it
            // into the signature (embed=false)
            signature.addDocument("./sample.txt", "text/plain", false);

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            signature.addKeyInfo(cert);

            // add signer role
            signature.QualifyingProperties.SignedSignatureProperties.SignerRole =
                new SignerRole(context, new[] {new ClaimedRole(context, "Manager")});

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}