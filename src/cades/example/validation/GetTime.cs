using System;
using System.Collections.Generic;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.cms;
using tr.gov.tubitak.uekae.esya.api.asn.pkixtsp;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.signature.attribute;
using tr.gov.tubitak.uekae.esya.asn.util;

/**
 * Get times from signature.
 */

namespace tr.gov.tubitak.uekae.esya.api.cades.example.validation
{
    [TestFixture]
    public class GetTime : CadesSampleBase
    {
        /**
         * Gets archive time stamp. It indicated then signature is converted to ESA.
         * @throws Exception
         */

        [Test]
        public void testarchiveTimestamp()
        {
            byte[] input = AsnIO.dosyadanOKU(getTestDataFolder() + "ESA-1.p7s");
            BaseSignedData bs = new BaseSignedData(input);
            List<TimestampInfo> timestampInfos = bs.getSignerList()[0].getAllArchiveTimeStamps();
            
            if (timestampInfos.Count == 0)
            {
                Assert.Fail("Could not find ETS attributes in the provided input file");
            }

            foreach (TimestampInfo timestampInfo in timestampInfos)
            {
                ETSTInfo tstInfo = timestampInfo.getTSTInfo();
                Console.WriteLine(tstInfo.getTime().ToString());
            }
        }

        /**
	 * Gets signature time stamp. It indicates when the sign was created.
	 * @throws Exception
	 */

        [Test]
        public void testSignatureTS()
        {
            byte[] input = AsnIO.dosyadanOKU(getTestDataFolder() + "EST-1.p7s");
            BaseSignedData bs = new BaseSignedData(input);
            EST estSign = (EST) bs.getSignerList()[0];
            DateTime? time = estSign.getTime();
            Console.WriteLine(time.ToString());
        }

        /**
         * Gets signing time attribute time. It indicates the declared time when the signature is created. 
         * @throws Exception
         */

        [Test]
        public void testSigningTime()
        {
            byte[] input = AsnIO.dosyadanOKU(getTestDataFolder() + "BES-2.p7s");

            BaseSignedData bs = new BaseSignedData(input);
            List<EAttribute> attrs = bs.getSignerList()[0].getSignedAttribute(AttributeOIDs.id_signingTime);
            ETime time = new ETime(attrs[0].getValue(0));
            Console.WriteLine(time.getTime().Value.ToLocalTime());
        }
    }
}
