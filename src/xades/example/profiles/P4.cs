using System;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.profiles
{
    /**
     * Profile 4 sample
     */

    [TestFixture]
    public class P4 : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "p4.xml";

        /**
         * Creates signature according to the profile 4 specifications
         */

        [Test]
        public void createP4()
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
            signature.setPolicyIdentifier(OID_POLICY_P4,
                "Uzun Dönemli ve ÇİSDuP Kontrollü Güvenli Elektronik İmza Politikası",
                "http://www.eimza.gov.tr/EimzaPolitikalari/216792161015070321.pdf"
            );

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

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