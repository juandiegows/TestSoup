// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Body {
    public ConsultaUbicaPlus consultaUbicaPlus { get; set; }
}

public class ConsultaUbicaPlus {
    public ParametrosUbica parametrosUbica { get; set; }
}

public class Envelope {
    public string Header { get; set; }
    public Body Body { get; set; }
}

public class ParametrosUbica {
    public string codigoInformacion { get; set; }
    public string motivoConsulta { get; set; }
    public string numeroIdentificacion { get; set; }
    public string primerApellido { get; set; }
    public string tipoIdentificacion { get; set; }
}

public class Root {
    public Envelope Envelope { get; set; }
}

