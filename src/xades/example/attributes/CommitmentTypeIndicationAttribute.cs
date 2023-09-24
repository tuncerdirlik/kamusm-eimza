using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.xades.example;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation;
using tr.gov.tubitak.uekae.esya.api.xmlsignature.model.xades;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.attributes
{
    /**
     * BES with CommitmentTypeIndication attribute sample
     */

    [TestFixture]
    internal class CommitmentTypeIndicationAttribute : XadesSampleBase
    {
        public static readonly string SIGNATURE_FILENAME = "commitment_type_indication.xml";

        private CommitmentTypeIndication createTestCTI(Context c, string ref1, string ref2)
        {
            CommitmentTypeId typeId = new CommitmentTypeId(
                c,
                new Identifier(c, "http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin", null),
                "Proof of origin indicates that the signer recognizes to have created, approved and sent the signed data object.",
                //Arrays.asList(
                new List<string>(new[]
                {
                    "http://test.test/commitment1.txt</xades:DocumentationReference",
                    "file:///test/data/xml/commitment2.txt</xades:DocumentationReference",
                    "http://test.test/commitment3.txt</xades:DocumentationReference"
                })
            );

            CommitmentTypeQualifier q1 = new CommitmentTypeQualifier(c);
            q1.addContent("test commitment a");
            q1.addContent(getQualifierSampleContent());
            q1.addContent("test commitment b");
            CommitmentTypeQualifier q2 = new CommitmentTypeQualifier(c, "commitment 2");
            CommitmentTypeQualifier q3 = new CommitmentTypeQualifier(c, "commitment 2");

            return new CommitmentTypeIndication(c, typeId,
                new List<string>(new[] {ref1, ref2}), false,
                new List<CommitmentTypeQualifier>(new[] {q1, q2, q3}));
        }

        private XmlElement getQualifierSampleContent()
        {
            return stringToElement(
                "<xl:XadesLabs xmlns:xl=\"http://xadeslabs.com/xades\"> \n" +
                "          <xl:Commitments type=\"ProofOfOrigin\">\n" +
                "            <xl:Commitment>commitment 1</xl:Commitment>\n" +
                "            <xl:Commitment>commitment 2</xl:Commitment>\n" +
                "            <xl:Commitment>commitment 3</xl:Commitment>\n" +
                "            <xl:Commitment>commitment 4</xl:Commitment>\n" +
                "          </xl:Commitments>\n" +
                "</xl:XadesLabs>");
        }

        private XmlElement stringToElement(string aStr)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(aStr);
            MemoryStream ms = new MemoryStream(bytes);

            XmlDocument doc = new XmlDocument();
            XmlReader reader = XmlReader.Create(ms);
            doc.Load(reader);

            return doc.DocumentElement;
        }

        /**
         * Creates detached BES with CommitmentTypeIndication attribute
         */

        [Test]
        public void createBESWithCommitmentTypeIndication()
        {
            // create context with working directory
            Context context = createContext();

            // create signature according to context,
            // with default type (XADES_BES)
            XMLSignature signature = new XMLSignature(context);

            // add document
            string ref1 = "#" + signature.addDocument("./sample.txt", "text/plain", true);
            string objId2 = signature.addPlainObject("Test data 1.", "text/plain", null);
            string ref2 = "#" + signature.addDocument("#" + objId2, null, false);

            // add certificate to show who signed the document
            // arrange the parameters whether the certificate is qualified or not
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
            signature.addKeyInfo(cert);

            // add commitment type indication
            signature.QualifyingProperties.SignedProperties.SignedDataObjectProperties.
                addCommitmentTypeIndication(createTestCTI(context, ref1, ref2));

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