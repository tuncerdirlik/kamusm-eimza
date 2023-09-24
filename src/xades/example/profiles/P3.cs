using System;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.signature.certval;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.document;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.profiles
{
    /**
     * Profile 3 sample
     */

    [TestFixture]
    public class P3 : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "p3.xml";

        /**
         * Creates T type signature according to the profile 3 specifications. It must be
         * upgraded to XL type using the second function (upgradeP3). If 'use-validation-
         * data-published-after-creation' is true in xml signature config file, after signing,
         * at least one new CRL for signing certificate must be published before upgrade.
         * @throws Exception
         */

        [Test]
        public void test()
        {
            createP3();

            Console.Out.WriteLine("Yeni sil yayınlandıktan sonra 'upgrade' işlemi yapılmalıdır.");

            upgradeP3();
        }

        public void createP3()
        {
            // create context with working directory
            Context context = createContext();

            // add resolver to resolve policies
            context.addExternalResolver(getPolicyResolver());

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

            // add signing time
            signature.SigningTime = DateTime.Now;

            // set policy info defined and required by profile
            signature.setPolicyIdentifier(OID_POLICY_P3,
                "Uzun Dönemli ve SİL Kontrollü Güvenli Elektronik İmza Politikası",
                "http://www.eimza.gov.tr/EimzaPolitikalari/216792161015070321.pdf"
            );

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            // upgrade to T
            signature.upgrade(api.signature.SignatureType.ES_T);

            FileStream fileStream = new FileStream(getTestDataFolder() + "p3_temp.xml", FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();
        }

        /**
         * Upgrades temporary T type signature to XL to create a signature with
         * profile 3 specifications. Do not run this upgrade code to be able to
         * validate using 'use-validation-data-published-after-creation' true
         * just afer creating temporary signature in the above function (createP3).
         * Also, revocation data must be CRL instead of OCSP in this profile.
         * @throws Exception
         */

        public void upgradeP3()
        {
            // create context with working dir
            Context context = createContext();

            // set policy such that it only works with CRL
            CertValidationPolicies policies = new CertValidationPolicies();
            policies.register(null, PolicyReader.readValidationPolicy(getPolicyFileCrl()));

            context.Config.ValidationConfig.setCertValidationPolicies(policies);

            // add resolver to resolve policies
            context.addExternalResolver(getPolicyResolver());

            // read temporary signature
            XMLSignature signature =
                XMLSignature.parse(new FileDocument(new FileInfo(getTestDataFolder() + "p3_temp.xml")), context);

            // upgrade to XL
            signature.upgrade(api.signature.SignatureType.ES_XL);

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}