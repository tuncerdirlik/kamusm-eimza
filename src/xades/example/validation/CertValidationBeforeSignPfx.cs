using System;
using System.IO;
using log4net.Config;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.common;
using tr.gov.tubitak.uekae.esya.api.xades.example;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation
{
    /**
     * Sample for signing a document only if certificate is valid
     */

    [TestFixture]
    public class CertValidationBeforeSignPfx : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "validate_before_sign.xml";


        [TestFixtureSetUp]
        public void setUp()
        {
            XmlConfigurator.Configure(new FileInfo(getTestDataFolder() + "config\\log4net.xml"));
        }

        /**
                                                                 * Creates BES with only valid certificates
                                                                 */

        [Test]
        public void createBESWithCertificateCheck()
        {
            ECertificate pfxSignersCertificate = getPfxSigner().getSignersCertificate();

            // check validity of signing certificate
            bool valid = CertValidation.validateCertificate(pfxSignersCertificate);

            if (valid)
            {
                // create context with working directory
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
                signature.addKeyInfo(pfxSignersCertificate);

                // now sign it by using signer
                signature.sign(getPfxSigner());

                FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
                signature.write(fileStream);
                fileStream.Close();

                XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
                signatureValidation.validate(SIGNATURE_FILENAME);
            }
            else
            {
                throw new ESYAException("Certificate " + pfxSignersCertificate + " is not a valid certificate!");
            }
        }
    }
}