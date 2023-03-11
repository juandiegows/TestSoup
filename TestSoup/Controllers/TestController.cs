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
                Body =new Body() {
                    consultaUbicaPlus = new ConsultaUbicaPlus() {
                        parametrosUbica = new ParametrosUbica(){
                            motivoConsulta = "24",
                            codigoInformacion = "5632",
                            numeroIdentificacion = "17809529",
                            primerApellido = "JIMENEZ",
                            tipoIdentificacion = "1"
                        }
                    }
                },
                Header  = ""
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
                var content = new StringContent(xmlData, Encoding.UTF8, "text/xml");

                var response = await client.PostAsync("https://miportafoliouat.transunion.co/ws/UbicaPlusWebService/services/UbicaPlus", content);
                var content2 = await response.Content.ReadAsStringAsync();
                return Ok(content2);

            }
            catch (Exception ex) {

                return BadRequest(ex.Message + " => " + certPath);
            }
            return Ok("HOla");
        }
    }
}
