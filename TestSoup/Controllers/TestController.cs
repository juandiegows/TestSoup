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

namespace TestSoup.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase {
        [HttpGet]
        public async Task<ActionResult> Index() {


            var certPath = Path.Combine("wwwroot", "ssl", "MetlifeTESTJK1.pfx");
            var password = "MetlifeTESTJK1";
            //string text = System.IO.File.ReadAllText(certPath.ToString());
            //return StatusCode(200, text);
            string username = "273562";
            string pass = "GspBuro2023#";
            var url = "https://miportafoliouat.transunion.co/ws/UbicaPlusWebService/services/UbicaPlus";

            Envelope data = new Envelope() {
                Body = new Body() {
                    consultaUbicaPlus = new ConsultaUbicaPlus() {
                        parametrosUbica = new ParametrosUbica() {
                            motivoConsulta = "24",
                            codigoInformacion = "5632",
                            numeroIdentificacion = "17809529",
                            primerApellido = "JIMENEZ",
                            tipoIdentificacion = "1"
                        }
                    }
                },
                Header = ""
            };


            var serializer = new XmlSerializer(typeof(Envelope));
            var stringWriter = new StringWriter();

            // Serializamos el objeto a XML y lo escribimos en el StringWriter
            serializer.Serialize(stringWriter, data);

            // Convertimos el StringWriter a un string
            var xmlData = stringWriter.ToString();
            try {

                var cert = new X509Certificate2(certPath, password);
                cert.FriendlyName = "MetlifeTESTJK1";
                var handler = new HttpClientHandler();

            
                //var sslContext = new SslStream(cert, false, (sender, certificate, chain, errors) => true); // Establece la cadena de certificados
                //handler.SslStreamFactory = () => sslContext;

                handler.ClientCertificates.Add(cert);



                var client = new HttpClient(handler);
                var authInfo = $"{username}:{pass}";

                var authInfoEncoded =
                    Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                //return StatusCode(200, authInfoEncoded);
                client.DefaultRequestHeaders.Add("SOAPAction", url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfoEncoded);
                client.DefaultRequestHeaders.TransferEncoding.Add(new TransferCodingHeaderValue("chunked"));

                string headertext = "";
                foreach (var item in client.DefaultRequestHeaders) {
                    headertext += $"{item.Key}: {string.Join(", ", item.Value)}\n";
                }

                //return StatusCode(200, headertext);

                var content = new StringContent(GetXmlString(), Encoding.UTF8, "application/xml");

                var response = await client.PostAsync(url, content);
                var content2 = await response.Content.ReadAsStringAsync();
         
                return StatusCode((int)response.StatusCode, content2);

            }
            catch (Exception ex) {

                return BadRequest(ex.Message + " => " + certPath);
            }
            return Ok("HOla");
        }

        private string GetXmlString() {
            string xmlString = @"<soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ubic=""http://ubicaplus.webservice.com"">
 <soapenv:Header>
<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
<wsu:Timestamp wsu:Id=""TS-7ECF631BE8AE9E5B09167752860077852"">
<wsu:Created>2023-03-11T19:10:00.778Z</wsu:Created>
<wsu:Expires>2023-03-27T22:56:40.778Z</wsu:Expires>
</wsu:Timestamp>


</wsse:Security>
</soapenv:Header>

   <soapenv:Body>
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
