using System;
using System.IO;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.model.xades;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.model.xades.timestamp;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.attributes
{
    /**
     * BES with IndividualDataObjectsTimeStamp attribute sample
     */

    [TestFixture]
    internal class IndividualDataObjectsTimeStampAttribute : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "individual_data_objects_timestamp.xml";

        /**
         * Creates detached BES with IndividualDataObjectsTimeStamp attribute
         */

        [Test]
        public void createBESWithIndividualDataObjectsTimeStamp()
        {
            // create context with working directory
            Context context = createContext();

            // create signature according to context,
            // with default type (XADES_BES)
            XMLSignature signature = new XMLSignature(context);

            // add document into the signature and get the reference
            string ref1 = "#" + signature.addDocument("./sample.txt", "text/plain", true);

            // add another object
            string objId2 = signature.addPlainObject("Test Data 1", "text/plain", null);
            string ref2 = "#" + signature.addDocument("#" + objId2, null, false);

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            signature.addKeyInfo(cert);

            // create new individual data objects timestamp structure
            IndividualDataObjectsTimeStamp timestamp = new IndividualDataObjectsTimeStamp(context);

            // add objects to timestamp structure
            timestamp.addInclude(new Include(context, ref1, true));
            timestamp.addInclude(new Include(context, ref2, true));

            // get encapsulated timestamp to individual data objects timestamp
            timestamp.addEncapsulatedTimeStamp(signature);

            // add individual data objects timestamp to signature
            signature.QualifyingProperties.SignedProperties.createOrGetSignedDataObjectProperties().
                addIndividualDataObjectsTimeStamp(timestamp);

            // optional - add timestamp validation data
            signature.addTimeStampValidationData(timestamp, DateTime.UtcNow);

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