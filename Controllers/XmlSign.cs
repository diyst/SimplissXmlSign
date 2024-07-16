using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using XmlSign.Models;

[ApiController]
[Route("api/[controller]")]
public class XmlSignController : ControllerBase
{

    private readonly X509Certificate2 _certificate;

    public XmlSignController(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    [HttpPost]
    [Consumes("application/json")]
    public IActionResult SignXml([FromBody] XmlSignRequest request)
    {
        RSA privateKey = _certificate.GetRSAPrivateKey();
        if (privateKey == null)
        {
            throw new CryptographicException("Não foi possível obter a chave privada do certificado.");
        }

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(request.XmlString);

        foreach (var referenceUri in request.NodesToSign)
        {
            SignedXml signedXml = new SignedXml(xmlDocument);
            signedXml.SigningKey = privateKey;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(_certificate));
            signedXml.KeyInfo = keyInfo;

            Reference reference = new Reference();
            reference.Uri = referenceUri;
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform(false));

            signedXml.AddReference(reference);
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
            reference.DigestMethod = SignedXml.XmlDsigSHA1Url;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode nodeToSign = xmlDocument.SelectSingleNode($"//*[@Id='{referenceUri.TrimStart('#')}']");
            if (nodeToSign != null && nodeToSign.ParentNode != null)
            {
                nodeToSign.ParentNode.InsertAfter(xmlDocument.ImportNode(xmlDigitalSignature, true), nodeToSign);
            }
        }

        return Ok(xmlDocument.OuterXml);
    }
}
