using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;


namespace TestSoup.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase {
        [HttpGet]
        public async Task<ActionResult> Index() {


            var certPath = Path.Combine("wwwroot", "ssl", "MetlifeTESTJK1.pfx");
            var password = "MetlifeTESTJK1";

            string username = "273562";
            string pass = "GspBuro2023#";


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
                var handler = new HttpClientHandler();

                handler.ClientCertificates.Add(cert);

                var client = new HttpClient(handler);
                var authInfo = $"{username}:{pass}";
                var authInfoEncoded = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfoEncoded);
                var content = new StringContent(GetXmlString(), Encoding.UTF8, "text/xml");

                var response = await client.PostAsync("https://miportafoliouat.transunion.co/ws/UbicaPlusWebService/services/UbicaPlus", content);
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
   <soapenv:Header/>
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
