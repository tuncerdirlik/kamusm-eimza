using System.IO;
using System.Xml;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.structures
{
    /**
     * Enveloped BES sample
     */

    [TestFixture]
    public class Enveloped : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "enveloped.xml";

        /**
         * Create enveloped BES
         */

        [Test]
        public void createEnveloped()
        {
            // here is our custom envelope xml
            XmlDocument envelopeDoc = newEnvelope();

            // create context with working dir
            Context context = createContext();

            // define where signature belongs to
            context.Document = envelopeDoc;

            // create signature according to context,
            // with default type (XADES_BES)
            XMLSignature signature = new XMLSignature(context, false);

            // attach signature to envelope
            envelopeDoc.DocumentElement.AppendChild(signature.Element);

            // add document as reference,
            signature.addDocument("#data1", "text/xml", false);

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            signature.addKeyInfo(cert);

            // now sign it by using smart card
            // specifiy the PIN before sign
            signature.sign(SmartCardManager.getInstance().getSigner(getPin(), cert));

            // this time we dont use signature.write because we need to write
            // whole document instead of signature
            Stream stream = new FileStream(getTestDataFolder() + SIGNATURE_FILENAME, FileMode.Create);
            /*if(!envelopeDoc.InnerXml.Contains(XmlUtil.XML_PREAMBLE_STR))
            {
                byte[] utf8Definition = XmlUtil.XML_PREAMBLE;
                s.Write(utf8Definition, 0, utf8Definition.Length);
            }*/
            envelopeDoc.Save(stream);
            stream.Close();

            XadesSignatureValidation signatureValidation = new XadesSignatureValidation();
            signatureValidation.validate(SIGNATURE_FILENAME);
        }
    }
}