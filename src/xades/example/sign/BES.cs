using System;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.crypto.parameters;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.sign
{
    /**
     * BES signing sample
     */

    [TestFixture]
    public class BES : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "bes.xml";

        /**
         * Creates detached BES
         */
        [Test]
        public void createBesWith_RSA_SHA256() 
        {
            testCreateEnveloping(Algorithms.SIGNATURE_RSA_SHA256, null);
        }

        [Test]
        public void createBesWith_RSA_PSS() 
        {
             testCreateEnveloping(Algorithms.SIGNATURE_RSA_PSS, new RSAPSSParams(DigestAlg.SHA256));
        }

        [Test]
        public void createBesWith_ECDSA_SHA384()
        {
            testCreateEnveloping(Algorithms.SIGNATURE_ECDSA_SHA384, null);
        }

        [Test]
        public void testSelectSigningAlgAccordingToCert()
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

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }

        public void testCreateEnveloping(String signingAlgorithm, IAlgorithmParams algParams)
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

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert, signingAlgorithm, algParams));

            FileStream fileStream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            signature.write(fileStream);
            fileStream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}