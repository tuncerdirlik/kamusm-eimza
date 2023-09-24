
using System;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.smartcard.pkcs11;

namespace tr.gov.tubitak.uekae.esya.api.smartcard.example
{
    public class ATR
    {
        [Test]
        public void testGetATR()
        {
            string [] ATRs = SmartOp.getCardATRs();
            for(int i = 0; i < ATRs.Length; i++)
                Console.WriteLine(ATRs[i]);
        }
    }
}
