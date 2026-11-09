using System.Text.Json.Serialization;

namespace Aureo.Domain.Entities
{
    // Serializado como texto ("Admin") para o front não depender de números mágicos.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PapelUsuario
    {
        Usuario = 0,
        Admin = 1
    }
}
