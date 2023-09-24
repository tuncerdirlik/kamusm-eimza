using System;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.asn.util;

/**
 * Gets signer certificate from BasedSignedData
 */

namespace tr.gov.tubitak.uekae.esya.api.cades.example.validation
{
    [TestFixture]
    public class GetSignerCertificate : CadesSampleBase
    {
        /***
	 * Gets certificate of the first signature.
	 * @throws Exception
	 */

        [Test]
        public void testGetCertificate()
        {
            byte[] sign = AsnIO.dosyadanOKU(getTestDataFolder() + "BES-1.p7s");
            BaseSignedData bs = new BaseSignedData(sign);
            Console.WriteLine("Certificate Owner: " +
                              bs.getSignerList()[0].getSignerCertificate().getSubject().getCommonNameAttribute());
        }
    }
}