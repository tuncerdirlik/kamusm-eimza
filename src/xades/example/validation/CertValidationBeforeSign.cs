using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.common;
using tr.gov.tubitak.uekae.esya.api.xades.example;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation
{
    /**
     * Sample for signing a document with smart card only if certificate is valid
     */

    public class CertValidationBeforeSign : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "validate_before_sign_smartcard.xml";

        /**
         * Creates BES using smart card with only valid certificates
         */

        [Test]
        public void createBESWithCertificateCheckSmartcard()
        {
            // false-true gets non-qualified certificates while true-false gets qualified ones
            ECertificate certificate = SmartCardManager.getInstance().getSignatureCertificate(isQualified());

            // check validity of signing certificate
            bool valid = CertValidation.validateCertificate(certificate);

            if (valid)
            {
                // create context with working dir
                Context context = createContext();

                /* optional - specifying policy from code
                // generate policy to be used in certificate validation
                ValidationPolicy policy = PolicyReader.readValidationPolicy(POLICY_FILE);

                CertValidationPolicies policies = new CertValidationPolicies();
                // null means default
                policies.register(null,policy);

                context.Config.ValidationConfig.setCertValidationPolicies(policies);
                */

                // create signature according to context,
                // with default type (XADES_BES)
                XMLSignature signature = new XMLSignature(context);

                // add document as reference, but do not embed it
                // into the signature (embed=false)
                signature.addDocument("./sample.txt", "text/plain", false);

                // add certificate to show who signed the document
                signature.addKeyInfo(certificate);

                // now sign it by using smart card
                signature.sign(SmartCardManager.getInstance().getSigner(getPin(), certificate));

                FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
                signature.write(fileStream);
                fileStream.Close();
            }
            else
            {
                throw new ESYAException("Certificate " + certificate.ToString() + " is not a valid certificate!");
            }
        }
    }
}