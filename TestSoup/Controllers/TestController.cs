using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Services3.Security.Tokens;

using System.Buffers;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Security.Authentication;
using System.Net.Security;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Net;
using Microsoft.Web.Services3;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace TestSoup.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase {
        String certPath = Path.Combine("wwwroot", "ssl", "MetlifeTESTJK1.pfx");
        String password = "MetlifeTESTJK1";
        //string text = System.IO.File.ReadAllText(certPath.ToString());
        //return StatusCode(200, text);
        string username = "273562";
        string pass = "GspBuro2023#";
        String url = "https://miportafoliouat.transunion.co/ws/UbicaPlusWebService/services/UbicaPlus";

        [HttpGet]
        public async Task<ActionResult> Index() {

            try {

                var cert = new X509Certificate2(certPath, password);
                cert.FriendlyName = "MetlifeTESTJK1".ToLower();

                var handler = new HttpClientHandler();

                handler.ClientCertificates.Add(cert);



                var client = new HttpClient(handler);
                var authInfo = $"{username}:{pass}";

                var authInfoEncoded =
                    Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                client.DefaultRequestHeaders.Add("SOAPAction", url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfoEncoded);
                client.DefaultRequestHeaders.TransferEncoding.Add(new TransferCodingHeaderValue("chunked"));

                //string headertext = "";
                //foreach (var item in client.DefaultRequestHeaders) {
                //    headertext += $"{item.Key}: {string.Join(", ", item.Value)}\n";
                //}

                ////return StatusCode(200, headertext);

                // Crea un objeto XmlDocument con el contenido de la solicitud SOAP
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(GetXmlString());

                SignedXml signedXml = new SignedXml(xmlDoc);
                signedXml.SigningKey = cert.PrivateKey;
                Reference reference = new Reference("#body");

                signedXml.AddReference(reference);

                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new KeyInfoX509Data(cert));
                signedXml.KeyInfo = keyInfo;

                signedXml.ComputeSignature();

                XmlElement signatureElement = signedXml.GetXml();
                XmlElement soapHeader = (XmlElement)(xmlDoc.GetElementsByTagName("wsse:Security")[0]);

                soapHeader.AppendChild(signatureElement);
                TimeSlamp(xmlDoc, soapHeader);

                var content = new StringContent(xmlDoc.OuterXml, Encoding.UTF8, "application/xml");

                var response = await client.PostAsync(url, content);
                var content2 = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, content2 + "\n " + xmlDoc.OuterXml);

            }
            catch (Exception ex) {

                return BadRequest(ex.Message + " => " + certPath);
            }
        }

        private static void TimeSlamp(XmlDocument xmlDoc, XmlElement soapHeader) {
            XmlElement timestamp = xmlDoc.CreateElement("wsu", "Timestamp", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            timestamp.SetAttribute("Id", "TS-7ECF631BE8AE9E5B09167752860077852");

            // Crea los elementos Created y Expires dentro del elemento Timestamp
            XmlElement created = xmlDoc.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            created.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            XmlElement expires = xmlDoc.CreateElement("wsu", "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            expires.InnerText = DateTime.UtcNow.AddSeconds(10).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Agrega los elementos Created y Expires al elemento Timestamp
            timestamp.AppendChild(created);
            timestamp.AppendChild(expires);
            soapHeader.AppendChild(timestamp);
        }

        [HttpGet("login")]
        public ActionResult Login() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/xml;charset=UTF-8";

            NetworkCredential credentials = new NetworkCredential(username, pass);
            request.Credentials = credentials;

            string authInfo = credentials.UserName + ":" + credentials.Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(GetXmlString());

            var cert = new X509Certificate2(certPath, password);
            cert.FriendlyName = "MetlifeTESTJK1";
            request.ClientCertificates.Add(cert);

            using (Stream stream = request.GetRequestStream()) {
                soapEnvelopeXml.Save(stream);
            }


            try {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                        string responseXml = reader.ReadToEnd();
                        // procesa la respuesta aquí
                        return StatusCode((int)response.StatusCode, responseXml);
                    }
                }
            }
            catch (Exception err) {

                return BadRequest(err.Message);
            }
            return Ok();
        }

        private string GetXmlString() {
            string xmlString = @"<soapenv:Envelope 
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
xmlns:ubic=""http://ubicaplus.webservice.com""
xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
>
  <soapenv:Header >
<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">


</wsse:Security>

  </soapenv:Header>


   <soapenv:Body id=""body"" >
      <ubic:consultaUbicaPlus soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
         <parametrosUbica xsi:type=""dto:ParametrosUbicaPlusDTO"" xmlns:dto=""http://dto.ubicaplus.webservice.com"">
            <codigoInformacion xsi:type=""soapenc:string"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">5632</codigoInformacion>
            <motivoConsulta xsi:type=""soapenc:string"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">24</motivoConsulta>
            <numeroIdentificacion xsi:type=""soapenc:string"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">17809529</numeroIdentificacion>
            <primerApellido xsi:type=""soapenc:string"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">JIMENEZ</primerApellido>
            <tipoIdentificacion xsi:type=""soapenc:string"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">1</tipoIdentificacion>
         </parametrosUbica>
      </ubic:consultaUbicaPlus>
   </soapenv:Body>
</soapenv:Envelope>";

            return xmlString;
        }

      

    }
}
