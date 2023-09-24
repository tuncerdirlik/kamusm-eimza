using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Objsys.Asn1.Runtime;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.cms;
using tr.gov.tubitak.uekae.esya.api.asn.profile;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;
using Attribute = tr.gov.tubitak.uekae.esya.asn.x509.Attribute;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.attributes
{
    [TestFixture]
    public class AttributeTest : CadesSampleBase
    {
        [Test]
        public void testAttribute()
        {
            BaseSignedData bs = new BaseSignedData();

            ISignable content = new SignableByteArray(Encoding.ASCII.GetBytes("test"));
            TSSettings tsSettings = getTSSettings();

            //add content which will be signed
            bs.addContent(content);

            DateTime? signingTimeAttr = DateTime.UtcNow;

            //create the claimed role attribute for signerattributes attribute
            EAttribute attr1 = new EAttribute(new Attribute());
            attr1.setType(new Asn1ObjectIdentifier(new[] {1, 3, 6, 7, 8, 10}));
            Asn1UTF8String role = new Asn1UTF8String("supervisor");
            Asn1DerEncodeBuffer encBuf = new Asn1DerEncodeBuffer();
            role.Encode(encBuf);
            attr1.addValue(encBuf.MsgCopy);
            EClaimedAttributes caAttr = new EClaimedAttributes(new[] {attr1});

            EContentHints chAttr = new EContentHints("text/plain", new Asn1ObjectIdentifier(new[] {1, 2, 840, 113549, 1, 7, 1}));
            EMimeType mimeTypeAttr = new EMimeType("text/plain");

            //Specified attributes are optional,add them to optional attributes list
            List<IAttribute> optionalAttributes = new List<IAttribute>();
            optionalAttributes.Add(new SigningTimeAttr(signingTimeAttr));
            optionalAttributes.Add(new SignerLocationAttr("TURKEY", "KOCAELİ", new[] {"TUBITAK UEKAE", "GEBZE"}));
            optionalAttributes.Add(new CommitmentTypeIndicationAttr(CommitmentType.CREATION));
            optionalAttributes.Add(new ContentHintsAttr(chAttr));
            optionalAttributes.Add(new SignerAttributesAttr(caAttr));
            optionalAttributes.Add(new ContentIdentifierAttr(Encoding.ASCII.GetBytes("PL123456789")));
            optionalAttributes.Add(new SignaturePolicyIdentifierAttr(TurkishESigProfile.P2_1));
            optionalAttributes.Add(new ContentTimeStampAttr());
            optionalAttributes.Add(new MimeTypeAttr(mimeTypeAttr));

            //create parameters necessary for signature creation
            Dictionary<string, object> params_ = new Dictionary<string, object>();

            params_[EParameters.P_VALIDATE_CERTIFICATE_BEFORE_SIGNING] = false;

            //By default, QC statement is checked,and signature wont be created if it is not a qualified certificate
            //By setting this parameter to false,user can use test certificates

            params_[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();

            //parameters for ContentTimeStamp attribute
            params_[EParameters.P_TSS_INFO] = tsSettings;

            //By default, QC statement is checked,and signature wont be created if it is not a 
            //qualified certificate. 
            bool checkQCStatement = isQualified();

            ECertificate cert = SmartCardManager.getInstance().getSignatureCertificate(checkQCStatement);
            BaseSigner signer = SmartCardManager.getInstance().getSigner(getPin(), cert);

            //add signer
            bs.addSigner(ESignatureType.TYPE_BES, cert, signer, optionalAttributes, params_);

            byte[] encoded = bs.getEncoded();

            BaseSignedData bsController = new BaseSignedData(encoded);
            Signer aSigner = bsController.getSignerList()[0];
            List<EAttribute> attrs;

            attrs = aSigner.getAttribute(SigningTimeAttr.OID);
            DateTime? st = SigningTimeAttr.toTime(attrs[0]);
            //because of fraction, it is not exactly equal
            Assert.AreEqual(true, signingTimeAttr.Value.Ticks - st.Value.Ticks < 1000*10000);

            attrs = aSigner.getAttribute(SignerLocationAttr.OID);
            ESignerLocation sl = SignerLocationAttr.toSignerLocation(attrs[0]);
            Assert.AreEqual("TURKEY", sl.getCountry());
            Assert.AreEqual("KOCAELİ", sl.getLocalityName());
            Assert.AreEqual(true, new[] {"TUBITAK UEKAE", "GEBZE"}.SequenceEqual(sl.getPostalAddress()));

            attrs = aSigner.getAttribute(SignerAttributesAttr.OID);
            ESignerAttribute sa = SignerAttributesAttr.toESignerAttribute(attrs[0]);
            Assert.AreEqual(true, sa.getElements()[0].getClaimedAttributes().Equals(caAttr));

            attrs = aSigner.getAttribute(ContentHintsAttr.OID);
            EContentHints ch = ContentHintsAttr.toContentHints(attrs[0]);
            Assert.AreEqual(true, ch.Equals(chAttr));

            attrs = aSigner.getAttribute(MimeTypeAttr.OID);
            EMimeType mt = MimeTypeAttr.toMimeType(attrs[0]);
            Assert.AreEqual(true, mt.Equals(mimeTypeAttr));

            attrs = aSigner.getAttribute(ContentIdentifierAttr.OID);
            byte[] ci = ContentIdentifierAttr.toIdentifier(attrs[0]);
            Assert.AreEqual(true, ci.SequenceEqual(Encoding.ASCII.GetBytes("PL123456789")));

            attrs = aSigner.getAttribute(CommitmentTypeIndicationAttr.OID);
            CommitmentType ct = CommitmentTypeIndicationAttr.toCommitmentType(attrs[0]);
            Assert.AreEqual(CommitmentType.CREATION, ct);

            attrs = aSigner.getAttribute(ContentTimeStampAttr.OID);
            DateTime? cal = ContentTimeStampAttr.toTime(attrs[0]);
            DateTime? now = DateTime.Now;

            //For the minor PC time errors, 10 minutes added.
            now = now.Value.Add(TimeSpan.FromMinutes(10));
            if (now < cal)
                throw new Exception("ContentTimeStampAttr error");
        }
    }
}