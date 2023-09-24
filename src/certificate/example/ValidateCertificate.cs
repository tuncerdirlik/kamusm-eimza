using System;
using System.IO;
using ImzaApiTest.src.tr.gov.tubitak.uekae.esya.api;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.certificate.validation;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.check.certificate;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.validation
{
    [TestFixture]
    public class ValidateCertificate : SampleBase
    {
        private static readonly string POLICY_FILE_NES = getRootDir() + @"\config\certval-policy.xml";
        private static readonly string POLICY_FILE_MM = getRootDir() + @"\config\certval-policy-malimuhur.xml";

        public ValidationPolicy getPolicy(string fileName)
        {
            return PolicyReader.readValidationPolicy(new FileStream(fileName, FileMode.Open));
        }

        [Test]
        public void testValidateMMCertificate()
        {
            bool QCStatement = false; //Unqualified certificate
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(QCStatement);

            ValidationSystem vs = CertificateValidation.createValidationSystem(getPolicy(POLICY_FILE_MM));
            vs.setBaseValidationTime(DateTime.UtcNow);
            CertificateStatusInfo csi = CertificateValidation.validateCertificate(vs, cert);
            if (csi.getCertificateStatus() != CertificateStatus.VALID)
                throw new Exception("Not Verified");
        }

        [Test]
        public void testValidateNESCertificate()
        {
            bool QCStatement = true; //Qualified certificate
            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(QCStatement);

            ValidationSystem vs = CertificateValidation.createValidationSystem(getPolicy(POLICY_FILE_NES));
            vs.setBaseValidationTime(DateTime.UtcNow);
            CertificateStatusInfo csi = CertificateValidation.validateCertificate(vs, cert);
            if (csi.getCertificateStatus() != CertificateStatus.VALID)
                throw new Exception("Not Verified");
        }
    }
}